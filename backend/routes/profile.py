from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity
from models.database import db
from models.user import User
from models.subscription import Subscription
from models.device import Device
from models.bookmark import Bookmark
from models.search_history import SearchHistory
from datetime import datetime

profile_bp = Blueprint('profile', __name__)

def get_device_identifier():
    """Generate a unique device identifier based on user agent and IP"""
    import hashlib
    user_agent = request.headers.get('User-Agent', '')
    ip_address = request.remote_addr
    device_string = f"{user_agent}_{ip_address}"
    return hashlib.md5(device_string.encode()).hexdigest()

@profile_bp.route('/', methods=['GET'])
@jwt_required()
def get_profile():
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        user = User.query.get(current_user_id)
        if not user:
            return jsonify({'message': 'User not found'}), 404
        
        # Get additional profile information
        profile_data = user.to_dict()
        
        # Get statistics
        total_bookmarks = Bookmark.query.filter_by(user_id=current_user_id).count()
        total_searches = SearchHistory.query.filter_by(user_id=current_user_id).count()
        
        # Get recent activity
        recent_searches = SearchHistory.query.filter_by(user_id=current_user_id)\
            .order_by(SearchHistory.created_at.desc())\
            .limit(5)\
            .all()
        
        profile_data.update({
            'statistics': {
                'total_bookmarks': total_bookmarks,
                'total_searches': total_searches
            },
            'recent_searches': [search.to_dict() for search in recent_searches]
        })
        
        return jsonify({'profile': profile_data}), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to retrieve profile'}), 500

@profile_bp.route('/subscription', methods=['GET'])
@jwt_required()
def get_subscription():
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        user = User.query.get(current_user_id)
        if not user:
            return jsonify({'message': 'User not found'}), 404
        
        subscription = user.subscription
        if not subscription:
            # Create default subscription
            subscription = Subscription(
                user_id=current_user_id,
                subscription_type='simple',
                is_active=True
            )
            db.session.add(subscription)
            db.session.commit()
        
        # Get current devices
        current_devices = Device.query.filter_by(user_id=current_user_id, is_active=True).all()
        
        # Get subscription limits
        from config.config import Config
        device_limit = (Config.PRO_DEVICE_LIMIT if subscription.subscription_type == 'pro' 
                       else Config.SIMPLE_DEVICE_LIMIT)
        
        return jsonify({
            'subscription': subscription.to_dict(),
            'device_usage': {
                'current_devices': len(current_devices),
                'device_limit': device_limit,
                'available_slots': max(0, device_limit - len(current_devices)),
                'devices': [device.to_dict() for device in current_devices]
            }
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to retrieve subscription information'}), 500

@profile_bp.route('/subscription', methods=['PUT'])
@jwt_required()
def update_subscription():
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        user = User.query.get(current_user_id)
        if not user:
            return jsonify({'message': 'User not found'}), 404
        
        data = request.get_json()
        subscription_type = data.get('subscription_type')
        
        if subscription_type not in ['simple', 'pro']:
            return jsonify({'message': 'Invalid subscription type'}), 400
        
        subscription = user.subscription
        if not subscription:
            subscription = Subscription(
                user_id=current_user_id,
                subscription_type=subscription_type,
                is_active=True
            )
            db.session.add(subscription)
        else:
            subscription.subscription_type = subscription_type
        
        db.session.commit()
        
        return jsonify({
            'message': 'Subscription updated successfully',
            'subscription': subscription.to_dict()
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'message': 'Failed to update subscription'}), 500

@profile_bp.route('/devices', methods=['GET'])
@jwt_required()
def get_devices():
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        devices = Device.query.filter_by(user_id=current_user_id).all()
        
        return jsonify({
            'devices': [device.to_dict() for device in devices],
            'current_device': device_id
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to retrieve devices'}), 500

@profile_bp.route('/devices/<int:device_id>', methods=['DELETE'])
@jwt_required()
def remove_device(device_id):
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id_current = get_device_identifier()
        device_current = Device.query.filter_by(user_id=current_user_id, device_id=device_id_current, is_active=True).first()
        if not device_current:
            return jsonify({'message': 'Device not registered'}), 403
        
        # Get device to remove
        device_to_remove = Device.query.filter_by(id=device_id, user_id=current_user_id).first()
        if not device_to_remove:
            return jsonify({'message': 'Device not found'}), 404
        
        # Cannot remove current device
        if device_to_remove.device_id == device_id_current:
            return jsonify({'message': 'Cannot remove current device'}), 400
        
        device_to_remove.is_active = False
        db.session.commit()
        
        return jsonify({
            'message': 'Device removed successfully',
            'device': device_to_remove.to_dict()
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'message': 'Failed to remove device'}), 500

@profile_bp.route('/devices/<int:device_id>', methods=['PUT'])
@jwt_required()
def update_device_name(device_id):
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id_current = get_device_identifier()
        device_current = Device.query.filter_by(user_id=current_user_id, device_id=device_id_current, is_active=True).first()
        if not device_current:
            return jsonify({'message': 'Device not registered'}), 403
        
        data = request.get_json()
        device_name = data.get('device_name', '').strip()
        
        if not device_name:
            return jsonify({'message': 'Device name is required'}), 400
        
        device_to_update = Device.query.filter_by(id=device_id, user_id=current_user_id).first()
        if not device_to_update:
            return jsonify({'message': 'Device not found'}), 404
        
        device_to_update.device_name = device_name
        db.session.commit()
        
        return jsonify({
            'message': 'Device name updated successfully',
            'device': device_to_update.to_dict()
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'message': 'Failed to update device name'}), 500

@profile_bp.route('/activity', methods=['GET'])
@jwt_required()
def get_activity():
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        # Get activity type
        activity_type = request.args.get('type', 'all')  # 'searches', 'bookmarks', 'all'
        limit = request.args.get('limit', 20, type=int)
        
        activities = []
        
        # Get search history
        if activity_type in ['all', 'searches']:
            searches = SearchHistory.query.filter_by(user_id=current_user_id)\
                .order_by(SearchHistory.created_at.desc())\
                .limit(limit)\
                .all()
            
            for search in searches:
                activities.append({
                    'type': 'search',
                    'data': search.to_dict(),
                    'timestamp': search.created_at.isoformat()
                })
        
        # Get bookmarks
        if activity_type in ['all', 'bookmarks']:
            bookmarks = Bookmark.query.filter_by(user_id=current_user_id)\
                .order_by(Bookmark.created_at.desc())\
                .limit(limit)\
                .all()
            
            for bookmark in bookmarks:
                activities.append({
                    'type': 'bookmark',
                    'data': bookmark.to_dict(),
                    'timestamp': bookmark.created_at.isoformat()
                })
        
        # Sort all activities by timestamp
        activities.sort(key=lambda x: x['timestamp'], reverse=True)
        
        # Limit results
        activities = activities[:limit]
        
        return jsonify({
            'activities': activities,
            'activity_type': activity_type
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to retrieve activity'}), 500

@profile_bp.route('/stats', methods=['GET'])
@jwt_required()
def get_profile_stats():
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        # Get various statistics
        total_bookmarks = Bookmark.query.filter_by(user_id=current_user_id).count()
        total_searches = SearchHistory.query.filter_by(user_id=current_user_id).count()
        
        # Get recent activity (last 30 days)
        from datetime import timedelta
        last_30d = datetime.utcnow() - timedelta(days=30)
        
        recent_bookmarks = Bookmark.query.filter(
            Bookmark.user_id == current_user_id,
            Bookmark.created_at >= last_30d
        ).count()
        
        recent_searches = SearchHistory.query.filter(
            SearchHistory.user_id == current_user_id,
            SearchHistory.created_at >= last_30d
        ).count()
        
        # Get device count
        device_count = Device.query.filter_by(user_id=current_user_id, is_active=True).count()
        
        # Get subscription info
        user = User.query.get(current_user_id)
        subscription = user.subscription
        
        return jsonify({
            'statistics': {
                'total_bookmarks': total_bookmarks,
                'total_searches': total_searches,
                'recent_bookmarks_30d': recent_bookmarks,
                'recent_searches_30d': recent_searches,
                'active_devices': device_count,
                'subscription_type': subscription.subscription_type if subscription else 'simple'
            }
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to retrieve profile statistics'}), 500