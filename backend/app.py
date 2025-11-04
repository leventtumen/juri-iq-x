from flask import Flask
from flask_cors import CORS
from flask_jwt_extended import JWTManager
from flask_limiter import Limiter
from flask_limiter.util import get_remote_address
from config.config import Config
from models.database import init_db
from routes.auth import auth_bp
from routes.documents import documents_bp
from routes.search import search_bp
from routes.bookmarks import bookmarks_bp
from routes.admin import admin_bp
from routes.profile import profile_bp
from services.scheduler import document_scheduler
import os
import logging
from datetime import datetime

def create_app():
    app = Flask(__name__)
    app.config.from_object(Config)
    
    # Setup logging
    logging.basicConfig(level=logging.INFO)
    
    # Initialize extensions
    CORS(app)
    jwt = JWTManager(app)
    limiter = Limiter(
        app=app,
        key_func=get_remote_address,
        default_limits=["200 per day", "50 per hour"]
    )
    
    # Initialize database
    init_db(app)
    
    # Initialize scheduler
    document_scheduler.init_app(app)
    document_scheduler.start()
    
    # Register blueprints
    app.register_blueprint(auth_bp, url_prefix='/api/auth')
    app.register_blueprint(documents_bp, url_prefix='/api/documents')
    app.register_blueprint(search_bp, url_prefix='/api/search')
    app.register_blueprint(bookmarks_bp, url_prefix='/api/bookmarks')
    app.register_blueprint(admin_bp, url_prefix='/api/admin')
    app.register_blueprint(profile_bp, url_prefix='/api/profile')
    
    # JWT error handlers
    @jwt.expired_token_loader
    def expired_token_callback(jwt_header, jwt_payload):
        return {'message': 'Token has expired', 'redirect_to_login': True}, 401
    
    @jwt.invalid_token_loader
    def invalid_token_callback(error):
        return {'message': 'Invalid token', 'redirect_to_login': True}, 401
    
    @jwt.unauthorized_loader
    def missing_token_callback(error):
        return {'message': 'Authorization token is required', 'redirect_to_login': True}, 401
    
    # Health check endpoint
    @app.route('/api/health', methods=['GET'])
    def health_check():
        return {'status': 'healthy', 'timestamp': datetime.utcnow().isoformat()}, 200
    
    # Cleanup on app shutdown
    @app.teardown_appcontext
    def cleanup_scheduler(error):
        if error:
            logging.error(f"Application error: {str(error)}")
    
    return app

if __name__ == '__main__':
    app = create_app()
    try:
        app.run(host='0.0.0.0', port=5000, debug=True)
    except KeyboardInterrupt:
        logging.info("Shutting down application...")
        document_scheduler.stop()
    except Exception as e:
        logging.error(f"Application error: {str(e)}")
        document_scheduler.stop()