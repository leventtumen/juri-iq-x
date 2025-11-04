from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity
from models.database import db
from models.user import User
from models.subscription import Subscription
from models.device import Device
from models.document import Document
from datetime import datetime, timedelta

admin_bp = Blueprint('admin', __name__)

def get_device_identifier():
    """Generate a unique device identifier based on user agent and IP"""
    import hashlib
    user_agent = request.headers.get('User-Agent', '')
    ip_address = request.remote_addr
    device_string = f"{user_agent}_{ip_address}"
    return hashlib.md5(device_string.encode()).hexdigest()

def admin_required(f):
    """Decorator to require admin access"""
    def decorated_function(*args, **kwargs):
        current_user_id = get_jwt_identity()
        user = User.query.get(current_user_id)
        
        if not user or not user.is_admin:
            return jsonify({'message': 'Admin access required'}), 403
        
        return f(*args, **kwargs)
    decorated_function.__name__ = f.__name__
    return decorated_function

@admin_bp.route('/dashboard', methods=['GET'])
@jwt_required()
@admin_required
def get_dashboard():
    try:
        # Get system statistics
        total_users = User.query.count()
        active_users = User.query.filter_by(is_active=True).count()
        admin_users = User.query.filter_by(is_admin=True).count()
        blacklisted_users = User.query.filter_by(is_blacklisted=True).count()
        
        # Subscription statistics
        simple_subscriptions = Subscription.query.filter_by(subscription_type='simple', is_active=True).count()
        pro_subscriptions = Subscription.query.filter_by(subscription_type='pro', is_active=True).count()
        
        # Document statistics
        total_documents = Document.query.count()
        processed_documents = Document.query.filter_by(processed=True).count()
        
        # Device statistics
        total_devices = Device.query.count()
        active_devices = Device.query.filter_by(is_active=True).count()
        
        # Recent activity (last 24 hours)
        last_24h = datetime.utcnow() - timedelta(hours=24)
        recent_logins = User.query.filter(User.last_login_attempt >= last_24h).count()
        new_devices = Device.query.filter(Device.created_at >= last_24h).count()
        
        return jsonify({
            'user_stats': {
                'total_users': total_users,
                'active_users': active_users,
                'admin_users': admin_users,
                'blacklisted_users': blacklisted_users,
                'recent_logins': recent_logins
            },
            'subscription_stats': {
                'simple_subscriptions': simple_subscriptions,
                'pro_subscriptions': pro_subscriptions
            },
            'document_stats': {
                'total_documents': total_documents,
                'processed_documents': processed_documents,
                'processing_percentage': round((processed_documents / total_documents * 100) if total_documents > 0 else 0, 2)
            },
            'device_stats': {
                'total_devices': total_devices,
                'active_devices': active_devices,
                'new_devices_24h': new_devices
            }
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to retrieve dashboard data'}), 500

@admin_bp.route('/users', methods=['GET'])
@jwt_required()
@admin_required
def get_users():
    try:
        # Get pagination parameters
        page = request.args.get('page', 1, type=int)
        per_page = request.args.get('per_page', 20, type=int)
        
        # Filter parameters
        is_active = request.args.get('is_active', type=bool)
        is_admin = request.args.get('is_admin', type=bool)
        is_blacklisted = request.args.get('is_blacklisted', type=bool)
        subscription_type = request.args.get('subscription_type')
        
        # Build query
        query = User.query
        
        if is_active is not None:
            query = query.filter_by(is_active=is_active)
        
        if is_admin is not None:
            query = query.filter_by(is_admin=is_admin)
        
        if is_blacklisted is not None:
            query = query.filter_by(is_blacklisted=is_blacklisted)
        
        if subscription_type:
            query = query.join(Subscription).filter_by(subscription_type=subscription_type)
        
        # Order by creation date (newest first)
        query = query.order_by(User.created_at.desc())
        
        # Paginate results
        users = query.paginate(
            page=page,
            per_page=min(per_page, 100),
            error_out=False
        )
        
        return jsonify({
            'users': [user.to_dict() for user in users.items],
            'pagination': {
                'page': users.page,
                'per_page': users.per_page,
                'total': users.total,
                'pages': users.pages,
                'has_next': users.has_next,
                'has_prev': users.has_prev
            }
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to retrieve users'}), 500

@admin_bp.route('/users/<int:user_id>', methods=['GET'])
@jwt_required()
@admin_required
def get_user_details(user_id):
    try:
        user = User.query.get(user_id)
        if not user:
            return jsonify({'message': 'User not found'}), 404
        
        # Get detailed user information
        user_data = user.to_dict()
        
        # Add additional admin-specific information
        user_data['login_attempts'] = user.failed_login_attempts
        user_data['last_login_attempt'] = user.last_login_attempt.isoformat() if user.last_login_attempt else None
        
        # Get device history
        devices = Device.query.filter_by(user_id=user_id).all()
        user_data['device_history'] = [device.to_dict() for device in devices]
        
        return jsonify({'user': user_data}), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to retrieve user details'}), 500

@admin_bp.route('/users/<int:user_id>/toggle-status', methods=['POST'])
@jwt_required()
@admin_required
def toggle_user_status(user_id):
    try:
        user = User.query.get(user_id)
        if not user:
            return jsonify({'message': 'User not found'}), 404
        
        # Prevent admin from deactivating themselves
        current_user_id = get_jwt_identity()
        if user_id == current_user_id:
            return jsonify({'message': 'Cannot modify your own status'}), 400
        
        data = request.get_json()
        action = data.get('action')  # 'activate', 'deactivate', 'blacklist', 'unblacklist'
        
        if action == 'activate':
            user.is_active = True
            message = 'User activated successfully'
        elif action == 'deactivate':
            user.is_active = False
            message = 'User deactivated successfully'
        elif action == 'blacklist':
            user.is_blacklisted = True
            user.is_active = False
            message = 'User blacklisted successfully'
        elif action == 'unblacklist':
            user.is_blacklisted = False
            message = 'User unblacklisted successfully'
        else:
            return jsonify({'message': 'Invalid action'}), 400
        
        db.session.commit()
        
        return jsonify({
            'message': message,
            'user': user.to_dict()
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'message': 'Failed to update user status'}), 500

@admin_bp.route('/users/<int:user_id>/subscription', methods=['PUT'])
@jwt_required()
@admin_required
def update_user_subscription(user_id):
    try:
        user = User.query.get(user_id)
        if not user:
            return jsonify({'message': 'User not found'}), 404
        
        data = request.get_json()
        subscription_type = data.get('subscription_type')
        is_active = data.get('is_active', True)
        
        if subscription_type not in ['simple', 'pro']:
            return jsonify({'message': 'Invalid subscription type'}), 400
        
        # Update or create subscription
        subscription = user.subscription
        if not subscription:
            subscription = Subscription(
                user_id=user_id,
                subscription_type=subscription_type,
                is_active=is_active
            )
            db.session.add(subscription)
        else:
            subscription.subscription_type = subscription_type
            subscription.is_active = is_active
        
        db.session.commit()
        
        return jsonify({
            'message': 'Subscription updated successfully',
            'subscription': subscription.to_dict()
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'message': 'Failed to update subscription'}), 500

@admin_bp.route('/users/<int:user_id>/devices', methods=['GET'])
@jwt_required()
@admin_required
def get_user_devices(user_id):
    try:
        user = User.query.get(user_id)
        if not user:
            return jsonify({'message': 'User not found'}), 404
        
        devices = Device.query.filter_by(user_id=user_id).all()
        
        return jsonify({
            'devices': [device.to_dict() for device in devices],
            'user': user.to_dict()
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to retrieve user devices'}), 500

@admin_bp.route('/users/<int:user_id>/devices/<int:device_id>/revoke', methods=['POST'])
@jwt_required()
@admin_required
def revoke_user_device(user_id, device_id):
    try:
        user = User.query.get(user_id)
        if not user:
            return jsonify({'message': 'User not found'}), 404
        
        device = Device.query.filter_by(id=device_id, user_id=user_id).first()
        if not device:
            return jsonify({'message': 'Device not found'}), 404
        
        device.is_active = False
        db.session.commit()
        
        return jsonify({
            'message': 'Device revoked successfully',
            'device': device.to_dict()
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'message': 'Failed to revoke device'}), 500

@admin_bp.route('/system/process-documents', methods=['POST'])
@jwt_required()
@admin_required
def trigger_document_processing():
    try:
        from services.document_processor import DocumentProcessor
        import os
        
        # Get documents folder path
        documents_folder = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(__file__))), 'sample-documents')
        
        processor = DocumentProcessor()
        processed_count, error_count = processor.process_all_documents(documents_folder)
        
        return jsonify({
            'message': 'Document processing completed',
            'processed_count': processed_count,
            'error_count': error_count
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to process documents'}), 500

@admin_bp.route('/system/stats', methods=['GET'])
@jwt_required()
@admin_required
def get_system_stats():
    try:
        # Get detailed system statistics
        stats = {}
        
        # User statistics by subscription type
        stats['users_by_subscription'] = db.session.query(
            Subscription.subscription_type,
            db.func.count(Subscription.id).label('count')
        ).filter_by(is_active=True).group_by(Subscription.subscription_type).all()
        
        # Device usage statistics
        stats['devices_per_user'] = db.session.query(
            db.func.count(Device.id).label('device_count'),
            db.func.count(db.func.distinct(Device.user_id)).label('user_count')
        ).filter_by(is_active=True).first()
        
        # Document processing statistics by file type
        stats['documents_by_type'] = db.session.query(
            Document.file_type,
            db.func.count(Document.id).label('total'),
            db.func.sum(db.cast(Document.processed, db.Integer)).label('processed')
        ).group_by(Document.file_type).all()
        
        # Recent activity (last 7 days)
        last_7d = datetime.utcnow() - timedelta(days=7)
        stats['recent_activity'] = {
            'new_users': User.query.filter(User.created_at >= last_7d).count(),
            'new_devices': Device.query.filter(Device.created_at >= last_7d).count(),
            'login_attempts': User.query.filter(User.last_login_attempt >= last_7d).count()
        }
        
        return jsonify({'stats': stats}), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to retrieve system statistics'}), 500