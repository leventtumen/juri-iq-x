from flask import Blueprint, request, jsonify
from flask_jwt_extended import create_access_token, jwt_required, get_jwt_identity
from datetime import datetime, timedelta
import hashlib
import uuid
from models.database import db
from models.user import User
from models.device import Device
from models.subscription import Subscription
from config.config import Config

auth_bp = Blueprint('auth', __name__)

def get_device_identifier():
    """Generate a unique device identifier based on user agent and IP"""
    user_agent = request.headers.get('User-Agent', '')
    ip_address = request.remote_addr
    device_string = f"{user_agent}_{ip_address}"
    return hashlib.md5(device_string.encode()).hexdigest()

@auth_bp.route('/login', methods=['POST'])
def login():
    try:
        data = request.get_json()
        email = data.get('email', '').strip()
        password = data.get('password', '')
        
        if not email or not password:
            return jsonify({'message': 'Email and password are required'}), 400
        
        # Check rate limiting
        user = User.query.filter_by(email=email).first()
        
        if user and user.is_blacklisted:
            return jsonify({'message': 'Account is blacklisted. Please contact support.'}), 403
        
        # Check failed login attempts
        if (user and user.failed_login_attempts >= Config.MAX_LOGIN_ATTEMPTS and
            user.last_login_attempt and
            (datetime.utcnow() - user.last_login_attempt).seconds < Config.LOGIN_ATTEMPT_TIMEOUT):
            return jsonify({'message': 'Too many failed login attempts. Please try again later.'}), 429
        
        # Authenticate user
        if user and user.check_password(password) and user.is_active:
            # Reset failed attempts
            user.failed_login_attempts = 0
            user.last_login_attempt = None
            db.session.commit()
            
            # Check device limits
            device_id = get_device_identifier()
            current_devices = Device.query.filter_by(user_id=user.id, is_active=True).count()
            
            # Get user's subscription
            subscription = user.subscription
            if not subscription:
                # Create default simple subscription
                subscription = Subscription(
                    user_id=user.id,
                    subscription_type='simple',
                    is_active=True
                )
                db.session.add(subscription)
                db.session.commit()
            
            device_limit = (Config.PRO_DEVICE_LIMIT if subscription.subscription_type == 'pro' 
                          else Config.SIMPLE_DEVICE_LIMIT)
            
            # Check if device is already registered
            existing_device = Device.query.filter_by(user_id=user.id, device_id=device_id).first()
            if not existing_device:
                if current_devices >= device_limit:
                    # Blacklist user if trying to exceed device limit
                    user.is_blacklisted = True
                    db.session.commit()
                    return jsonify({'message': 'Device limit exceeded. Account has been blacklisted.'}), 403
                
                # Register new device
                new_device = Device(
                    user_id=user.id,
                    device_id=device_id,
                    device_name=data.get('device_name', 'Unknown Device'),
                    user_agent=request.headers.get('User-Agent'),
                    ip_address=request.remote_addr
                )
                db.session.add(new_device)
            else:
                # Update last seen for existing device
                existing_device.last_seen = datetime.utcnow()
                existing_device.is_active = True
            
            db.session.commit()
            
            # Create access token
            access_token = create_access_token(identity=user.id)
            
            return jsonify({
                'access_token': access_token,
                'user': user.to_dict()
            }), 200
        
        # Failed login attempt
        if user:
            user.failed_login_attempts += 1
            user.last_login_attempt = datetime.utcnow()
            
            # Blacklist if too many attempts
            if user.failed_login_attempts >= Config.MAX_LOGIN_ATTEMPTS:
                user.is_blacklisted = True
            
            db.session.commit()
        
        return jsonify({'message': 'Invalid email or password'}), 401
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'message': 'Login failed. Please try again.'}), 500

@auth_bp.route('/register', methods=['POST'])
def register():
    try:
        data = request.get_json()
        email = data.get('email', '').strip()
        password = data.get('password', '')
        subscription_type = data.get('subscription_type', 'simple')
        
        if not email or not password:
            return jsonify({'message': 'Email and password are required'}), 400
        
        # Check if user already exists
        if User.query.filter_by(email=email).first():
            return jsonify({'message': 'Email already registered'}), 409
        
        # Create new user
        user = User(
            email=email,
            is_admin=False,
            is_active=True
        )
        user.set_password(password)
        
        db.session.add(user)
        db.session.flush()  # Get user ID
        
        # Create subscription
        subscription = Subscription(
            user_id=user.id,
            subscription_type=subscription_type,
            is_active=True
        )
        db.session.add(subscription)
        
        # Register device
        device_id = get_device_identifier()
        device = Device(
            user_id=user.id,
            device_id=device_id,
            device_name=data.get('device_name', 'Registration Device'),
            user_agent=request.headers.get('User-Agent'),
            ip_address=request.remote_addr
        )
        db.session.add(device)
        
        db.session.commit()
        
        # Create access token
        access_token = create_access_token(identity=user.id)
        
        return jsonify({
            'access_token': access_token,
            'user': user.to_dict()
        }), 201
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'message': 'Registration failed. Please try again.'}), 500

@auth_bp.route('/logout', methods=['POST'])
@jwt_required()
def logout():
    try:
        user_id = get_jwt_identity()
        device_id = get_device_identifier()
        
        # Deactivate device
        device = Device.query.filter_by(user_id=user_id, device_id=device_id).first()
        if device:
            device.is_active = False
            db.session.commit()
        
        return jsonify({'message': 'Logged out successfully'}), 200
        
    except Exception as e:
        return jsonify({'message': 'Logout failed'}), 500

@auth_bp.route('/refresh', methods=['POST'])
@jwt_required()
def refresh():
    try:
        current_user_id = get_jwt_identity()
        user = User.query.get(current_user_id)
        
        if not user or not user.is_active:
            return jsonify({'message': 'User not found or inactive'}), 404
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        # Create new access token
        access_token = create_access_token(identity=current_user_id)
        
        return jsonify({
            'access_token': access_token,
            'user': user.to_dict()
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Token refresh failed'}), 500

@auth_bp.route('/me', methods=['GET'])
@jwt_required()
def get_current_user():
    try:
        current_user_id = get_jwt_identity()
        user = User.query.get(current_user_id)
        
        if not user:
            return jsonify({'message': 'User not found'}), 404
        
        return jsonify({'user': user.to_dict()}), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to get user info'}), 500