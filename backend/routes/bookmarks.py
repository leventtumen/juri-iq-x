from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity
from models.database import db
from models.bookmark import Bookmark
from models.document import Document
from models.user import User
from models.device import Device

bookmarks_bp = Blueprint('bookmarks', __name__)

def get_device_identifier():
    """Generate a unique device identifier based on user agent and IP"""
    import hashlib
    user_agent = request.headers.get('User-Agent', '')
    ip_address = request.remote_addr
    device_string = f"{user_agent}_{ip_address}"
    return hashlib.md5(device_string.encode()).hexdigest()

@bookmarks_bp.route('/', methods=['POST'])
@jwt_required()
def create_bookmark():
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        data = request.get_json()
        document_id = data.get('document_id')
        notes = data.get('notes', '').strip()
        
        if not document_id:
            return jsonify({'message': 'Document ID is required'}), 400
        
        # Check if document exists
        document = Document.query.get(document_id)
        if not document:
            return jsonify({'message': 'Document not found'}), 404
        
        # Check if bookmark already exists
        existing_bookmark = Bookmark.query.filter_by(
            user_id=current_user_id,
            document_id=document_id
        ).first()
        
        if existing_bookmark:
            return jsonify({'message': 'Document already bookmarked'}), 409
        
        # Create bookmark
        bookmark = Bookmark(
            user_id=current_user_id,
            document_id=document_id,
            notes=notes if notes else None
        )
        
        db.session.add(bookmark)
        db.session.commit()
        
        return jsonify({
            'message': 'Bookmark created successfully',
            'bookmark': bookmark.to_dict()
        }), 201
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'message': 'Failed to create bookmark'}), 500

@bookmarks_bp.route('/', methods=['GET'])
@jwt_required()
def get_bookmarks():
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        # Get pagination parameters
        page = request.args.get('page', 1, type=int)
        per_page = request.args.get('per_page', 20, type=int)
        
        # Order by creation date (newest first)
        bookmarks_query = Bookmark.query.filter_by(user_id=current_user_id)\
            .order_by(Bookmark.created_at.desc())
        
        # Paginate results
        bookmarks = bookmarks_query.paginate(
            page=page,
            per_page=min(per_page, 100),
            error_out=False
        )
        
        return jsonify({
            'bookmarks': [bookmark.to_dict() for bookmark in bookmarks.items],
            'pagination': {
                'page': bookmarks.page,
                'per_page': bookmarks.per_page,
                'total': bookmarks.total,
                'pages': bookmarks.pages,
                'has_next': bookmarks.has_next,
                'has_prev': bookmarks.has_prev
            }
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to retrieve bookmarks'}), 500

@bookmarks_bp.route('/<int:bookmark_id>', methods=['GET'])
@jwt_required()
def get_bookmark(bookmark_id):
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        bookmark = Bookmark.query.filter_by(
            id=bookmark_id,
            user_id=current_user_id
        ).first()
        
        if not bookmark:
            return jsonify({'message': 'Bookmark not found'}), 404
        
        return jsonify({'bookmark': bookmark.to_dict()}), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to retrieve bookmark'}), 500

@bookmarks_bp.route('/<int:bookmark_id>', methods=['PUT'])
@jwt_required()
def update_bookmark(bookmark_id):
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        bookmark = Bookmark.query.filter_by(
            id=bookmark_id,
            user_id=current_user_id
        ).first()
        
        if not bookmark:
            return jsonify({'message': 'Bookmark not found'}), 404
        
        data = request.get_json()
        notes = data.get('notes', '').strip()
        
        # Update bookmark notes
        bookmark.notes = notes if notes else None
        
        db.session.commit()
        
        return jsonify({
            'message': 'Bookmark updated successfully',
            'bookmark': bookmark.to_dict()
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'message': 'Failed to update bookmark'}), 500

@bookmarks_bp.route('/<int:bookmark_id>', methods=['DELETE'])
@jwt_required()
def delete_bookmark(bookmark_id):
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        bookmark = Bookmark.query.filter_by(
            id=bookmark_id,
            user_id=current_user_id
        ).first()
        
        if not bookmark:
            return jsonify({'message': 'Bookmark not found'}), 404
        
        db.session.delete(bookmark)
        db.session.commit()
        
        return jsonify({'message': 'Bookmark deleted successfully'}), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'message': 'Failed to delete bookmark'}), 500

@bookmarks_bp.route('/document/<int:document_id>', methods=['GET'])
@jwt_required()
def get_bookmark_by_document(document_id):
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        bookmark = Bookmark.query.filter_by(
            user_id=current_user_id,
            document_id=document_id
        ).first()
        
        if not bookmark:
            return jsonify({'bookmarked': False}), 200
        
        return jsonify({
            'bookmarked': True,
            'bookmark': bookmark.to_dict()
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to check bookmark status'}), 500

@bookmarks_bp.route('/stats', methods=['GET'])
@jwt_required()
def get_bookmark_stats():
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        total_bookmarks = Bookmark.query.filter_by(user_id=current_user_id).count()
        
        # Get recent bookmarks (last 7 days)
        from datetime import datetime, timedelta
        week_ago = datetime.utcnow() - timedelta(days=7)
        recent_bookmarks = Bookmark.query.filter(
            Bookmark.user_id == current_user_id,
            Bookmark.created_at >= week_ago
        ).count()
        
        return jsonify({
            'total_bookmarks': total_bookmarks,
            'recent_bookmarks': recent_bookmarks
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to retrieve bookmark statistics'}), 500