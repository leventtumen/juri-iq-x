from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity
from models.database import db
from models.document import Document
from models.user import User
from models.device import Device

documents_bp = Blueprint('documents', __name__)

def get_device_identifier():
    """Generate a unique device identifier based on user agent and IP"""
    import hashlib
    user_agent = request.headers.get('User-Agent', '')
    ip_address = request.remote_addr
    device_string = f"{user_agent}_{ip_address}"
    return hashlib.md5(device_string.encode()).hexdigest()

@documents_bp.route('/', methods=['GET'])
@jwt_required()
def get_documents():
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
        
        # Filter parameters
        file_type = request.args.get('file_type')
        processed = request.args.get('processed', type=bool)
        
        # Build query
        query = Document.query
        
        if file_type:
            query = query.filter_by(file_type=file_type)
        
        if processed is not None:
            query = query.filter_by(processed=processed)
        
        # Paginate results
        documents = query.paginate(
            page=page,
            per_page=min(per_page, 100),  # Limit max per_page to 100
            error_out=False
        )
        
        return jsonify({
            'documents': [doc.to_dict() for doc in documents.items],
            'pagination': {
                'page': documents.page,
                'per_page': documents.per_page,
                'total': documents.total,
                'pages': documents.pages,
                'has_next': documents.has_next,
                'has_prev': documents.has_prev
            }
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to retrieve documents'}), 500

@documents_bp.route('/<int:document_id>', methods=['GET'])
@jwt_required()
def get_document(document_id):
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        document = Document.query.get(document_id)
        if not document:
            return jsonify({'message': 'Document not found'}), 404
        
        return jsonify({'document': document.to_dict()}), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to retrieve document'}), 500

@documents_bp.route('/search', methods=['GET'])
@jwt_required()
def search_documents():
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        query_text = request.args.get('q', '').strip()
        if not query_text:
            return jsonify({'message': 'Search query is required'}), 400
        
        # Get pagination parameters
        page = request.args.get('page', 1, type=int)
        per_page = request.args.get('per_page', 20, type=int)
        
        # Simple text search in document content
        # For more advanced search, we could implement full-text search or vector similarity
        from models.document import DocumentContent
        
        documents_query = Document.query.join(DocumentContent).filter(
            Document.processed == True,
            db.or_(
                Document.original_filename.contains(query_text),
                DocumentContent.raw_text.contains(query_text),
                DocumentContent.summary.contains(query_text),
                DocumentContent.keywords.contains(query_text)
            )
        )
        
        # Paginate results
        documents = documents_query.paginate(
            page=page,
            per_page=min(per_page, 100),
            error_out=False
        )
        
        return jsonify({
            'documents': [doc.to_dict() for doc in documents.items],
            'pagination': {
                'page': documents.page,
                'per_page': documents.per_page,
                'total': documents.total,
                'pages': documents.pages,
                'has_next': documents.has_next,
                'has_prev': documents.has_prev
            },
            'query': query_text
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Search failed'}), 500

@documents_bp.route('/types', methods=['GET'])
@jwt_required()
def get_document_types():
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        # Get unique file types
        file_types = db.session.query(Document.file_type).distinct().all()
        file_type_list = [ft[0] for ft in file_types if ft[0]]
        
        # Get counts for each type
        type_counts = {}
        for file_type in file_type_list:
            count = Document.query.filter_by(file_type=file_type).count()
            type_counts[file_type] = count
        
        return jsonify({
            'types': file_type_list,
            'counts': type_counts
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to retrieve document types'}), 500

@documents_bp.route('/stats', methods=['GET'])
@jwt_required()
def get_document_stats():
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        # Get overall statistics
        total_docs = Document.query.count()
        processed_docs = Document.query.filter_by(processed=True).count()
        unprocessed_docs = total_docs - processed_docs
        
        # Get file type statistics
        file_type_stats = {}
        file_types = db.session.query(Document.file_type).distinct().all()
        for ft in file_types:
            if ft[0]:
                count = Document.query.filter_by(file_type=ft[0]).count()
                processed_count = Document.query.filter_by(file_type=ft[0], processed=True).count()
                file_type_stats[ft[0]] = {
                    'total': count,
                    'processed': processed_count,
                    'unprocessed': count - processed_count
                }
        
        return jsonify({
            'total_documents': total_docs,
            'processed_documents': processed_docs,
            'unprocessed_documents': unprocessed_docs,
            'processing_percentage': round((processed_docs / total_docs * 100) if total_docs > 0 else 0, 2),
            'file_type_stats': file_type_stats
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to retrieve document statistics'}), 500