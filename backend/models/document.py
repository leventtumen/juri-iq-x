from models.database import db
from datetime import datetime

class Document(db.Model):
    __tablename__ = 'documents'
    
    id = db.Column(db.Integer, primary_key=True)
    filename = db.Column(db.String(255), nullable=False)
    original_filename = db.Column(db.String(255), nullable=False)
    file_path = db.Column(db.String(500), nullable=False)
    file_type = db.Column(db.String(10), nullable=False)  # pdf, doc, docx, dot, txt
    file_size = db.Column(db.Integer)
    processed = db.Column(db.Boolean, default=False)
    created_at = db.Column(db.DateTime, default=datetime.utcnow)
    updated_at = db.Column(db.DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)
    
    # Relationships
    content = db.relationship('DocumentContent', backref='document', uselist=False)
    bookmarks = db.relationship('Bookmark', backref='document', lazy=True)
    
    def to_dict(self):
        return {
            'id': self.id,
            'filename': self.filename,
            'original_filename': self.original_filename,
            'file_type': self.file_type,
            'file_size': self.file_size,
            'processed': self.processed,
            'created_at': self.created_at.isoformat() if self.created_at else None,
            'updated_at': self.updated_at.isoformat() if self.updated_at else None,
            'content': self.content.to_dict() if self.content else None
        }

class DocumentContent(db.Model):
    __tablename__ = 'document_content'
    
    id = db.Column(db.Integer, primary_key=True)
    document_id = db.Column(db.Integer, db.ForeignKey('documents.id'), nullable=False)
    raw_text = db.Column(db.Text)
    summary = db.Column(db.Text)
    keywords = db.Column(db.Text)  # JSON string of keywords
    word_count = db.Column(db.Integer)
    processing_date = db.Column(db.DateTime, default=datetime.utcnow)
    
    def to_dict(self):
        import json
        return {
            'id': self.id,
            'document_id': self.document_id,
            'summary': self.summary,
            'keywords': json.loads(self.keywords) if self.keywords else [],
            'word_count': self.word_count,
            'processing_date': self.processing_date.isoformat() if self.processing_date else None
        }
</create_file>