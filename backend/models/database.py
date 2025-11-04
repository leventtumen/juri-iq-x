from flask_sqlalchemy import SQLAlchemy
from datetime import datetime
from werkzeug.security import generate_password_hash, check_password_hash

db = SQLAlchemy()

def init_db(app):
    db.init_app(app)
    with app.app_context():
        db.create_all()
        # Create default admin user
        create_admin_user()

def create_admin_user():
    from models.user import User
    from models.subscription import Subscription
    
    admin = User.query.filter_by(email='admin@test.com').first()
    if not admin:
        admin = User(
            email='admin@test.com',
            password_hash=generate_password_hash('Pass!2345'),
            is_admin=True,
            is_active=True,
            created_at=datetime.utcnow()
        )
        db.session.add(admin)
        
        # Give admin pro subscription
        pro_subscription = Subscription(
            user_id=admin.id,
            subscription_type='pro',
            is_active=True,
            created_at=datetime.utcnow()
        )
        db.session.add(pro_subscription)
        db.session.commit()