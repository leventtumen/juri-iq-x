from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity
from models.database import db
from models.document import Document, DocumentContent
from models.search_history import SearchHistory
from models.user import User
from models.device import Device
from services.document_processor import DocumentProcessor
from datetime import datetime

search_bp = Blueprint('search', __name__)

def get_device_identifier():
    """Generate a unique device identifier based on user agent and IP"""
    import hashlib
    user_agent = request.headers.get('User-Agent', '')
    ip_address = request.remote_addr
    device_string = f"{user_agent}_{ip_address}"
    return hashlib.md5(device_string.encode()).hexdigest()

@search_bp.route('/query', methods=['POST'])
@jwt_required()
def search_documents():
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        data = request.get_json()
        query = data.get('query', '').strip()
        
        if not query:
            return jsonify({'message': 'Search query is required'}), 400
        
        # Get pagination parameters
        page = data.get('page', 1)
        per_page = data.get('per_page', 20)
        similarity_threshold = data.get('similarity_threshold', 0.1)  # Default 10% similarity
        
        # Initialize document processor for similarity calculation
        processor = DocumentProcessor()
        
        # Get all processed documents
        processed_documents = Document.query.filter_by(processed=True).all()
        
        results = []
        for doc in processed_documents:
            if doc.content and doc.content.raw_text:
                # Calculate similarity scores
                title_similarity = processor.calculate_similarity(query, doc.original_filename)
                summary_similarity = processor.calculate_similarity(query, doc.content.summary or '')
                content_similarity = processor.calculate_similarity(query, doc.content.raw_text)
                
                # Calculate overall similarity (weighted average)
                overall_similarity = (
                    title_similarity * 0.3 +      # Title weight: 30%
                    summary_similarity * 0.4 +    # Summary weight: 40%
                    content_similarity * 0.3      # Content weight: 30%
                )
                
                # Only include documents above threshold
                if overall_similarity >= similarity_threshold:
                    results.append({
                        'document': doc.to_dict(),
                        'similarity_scores': {
                            'title': round(title_similarity * 100, 2),
                            'summary': round(summary_similarity * 100, 2),
                            'content': round(content_similarity * 100, 2),
                            'overall': round(overall_similarity * 100, 2)
                        }
                    })
        
        # Sort by overall similarity (descending)
        results.sort(key=lambda x: x['similarity_scores']['overall'], reverse=True)
        
        # Paginate results
        start_idx = (page - 1) * per_page
        end_idx = start_idx + per_page
        paginated_results = results[start_idx:end_idx]
        
        # Save search to history
        search_history = SearchHistory(
            user_id=current_user_id,
            query=query,
            results_count=len(results)
        )
        db.session.add(search_history)
        db.session.commit()
        
        return jsonify({
            'results': paginated_results,
            'pagination': {
                'page': page,
                'per_page': per_page,
                'total': len(results),
                'pages': (len(results) + per_page - 1) // per_page,
                'has_next': end_idx < len(results),
                'has_prev': page > 1
            },
            'query': query,
            'similarity_threshold': similarity_threshold
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Search failed'}), 500

@search_bp.route('/history', methods=['GET'])
@jwt_required()
def get_search_history():
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        # Get 10 most recent searches
        recent_searches = SearchHistory.query.filter_by(user_id=current_user_id)\
            .order_by(SearchHistory.created_at.desc())\
            .limit(10)\
            .all()
        
        return jsonify({
            'search_history': [search.to_dict() for search in recent_searches]
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to retrieve search history'}), 500

@search_bp.route('/suggestions', methods=['GET'])
@jwt_required()
def get_search_suggestions():
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        query = request.args.get('q', '').strip()
        
        if not query or len(query) < 2:
            return jsonify({'suggestions': []}), 200
        
        # Get suggestions from document titles and keywords
        suggestions = []
        
        # Search in document titles
        title_matches = Document.query.filter(
            Document.original_filename.contains(query),
            Document.processed == True
        ).limit(5).all()
        
        for doc in title_matches:
            suggestions.append({
                'text': doc.original_filename,
                'type': 'document',
                'document_id': doc.id
            })
        
        # Search in keywords
        keyword_matches = DocumentContent.query.filter(
            DocumentContent.keywords.contains(query)
        ).limit(5).all()
        
        for content in keyword_matches:
            if content.document:
                import json
                try:
                    keywords = json.loads(content.keywords) if content.keywords else []
                    for keyword in keywords:
                        if query.lower() in keyword.lower() and len(suggestions) < 10:
                            suggestions.append({
                                'text': keyword,
                                'type': 'keyword',
                                'document_id': content.document_id
                            })
                            break
                except:
                    pass
        
        return jsonify({'suggestions': suggestions}), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to get search suggestions'}), 500

@search_bp.route('/similar/<int:document_id>', methods=['GET'])
@jwt_required()
def get_similar_documents(document_id):
    try:
        current_user_id = get_jwt_identity()
        
        # Verify device
        device_id = get_device_identifier()
        device = Device.query.filter_by(user_id=current_user_id, device_id=device_id, is_active=True).first()
        if not device:
            return jsonify({'message': 'Device not registered'}), 403
        
        # Get the reference document
        reference_doc = Document.query.get(document_id)
        if not reference_doc or not reference_doc.processed or not reference_doc.content:
            return jsonify({'message': 'Document not found or not processed'}), 404
        
        # Get similarity threshold
        similarity_threshold = request.args.get('threshold', 0.2)  # Default 20% similarity
        limit = request.args.get('limit', 10, type=int)
        
        # Initialize document processor
        processor = DocumentProcessor()
        
        # Get all other processed documents
        other_documents = Document.query.filter(
            Document.processed == True,
            Document.id != document_id
        ).all()
        
        # Calculate similarities
        similar_docs = []
        ref_text = reference_doc.content.raw_text or ''
        
        for doc in other_documents:
            if doc.content and doc.content.raw_text:
                similarity = processor.calculate_similarity(ref_text, doc.content.raw_text)
                
                if similarity >= similarity_threshold:
                    similar_docs.append({
                        'document': doc.to_dict(),
                        'similarity_percentage': round(similarity * 100, 2)
                    })
        
        # Sort by similarity and limit results
        similar_docs.sort(key=lambda x: x['similarity_percentage'], reverse=True)
        similar_docs = similar_docs[:limit]
        
        return jsonify({
            'reference_document': reference_doc.to_dict(),
            'similar_documents': similar_docs,
            'similarity_threshold': similarity_threshold
        }), 200
        
    except Exception as e:
        return jsonify({'message': 'Failed to find similar documents'}), 500