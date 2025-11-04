from apscheduler.schedulers.background import BackgroundScheduler
from apscheduler.triggers.interval import IntervalTrigger
import os
import logging
from datetime import datetime
from services.document_processor import DocumentProcessor
from models.database import db
from models.document import Document

class DocumentScheduler:
    def __init__(self, app=None):
        self.scheduler = None
        self.app = app
        self.document_processor = DocumentProcessor()
        
    def init_app(self, app):
        self.app = app
        
    def start(self):
        if self.scheduler is None:
            self.scheduler = BackgroundScheduler()
            
            # Schedule document processing every 24 hours
            self.scheduler.add_job(
                func=self.process_documents,
                trigger=IntervalTrigger(hours=24),
                id='process_documents_job',
                name='Process new and updated documents',
                replace_existing=True
            )
            
            # Schedule cleanup of old inactive devices every week
            self.scheduler.add_job(
                func=self.cleanup_old_devices,
                trigger=IntervalTrigger(days=7),
                id='cleanup_devices_job',
                name='Cleanup old inactive devices',
                replace_existing=True
            )
            
            self.scheduler.start()
            logging.info("Document scheduler started")
    
    def stop(self):
        if self.scheduler:
            self.scheduler.shutdown()
            logging.info("Document scheduler stopped")
    
    def process_documents(self):
        """Process all documents in the sample-documents folder"""
        try:
            with self.app.app_context():
                # Get documents folder path
                documents_folder = os.path.join(
                    os.path.dirname(os.path.dirname(os.path.dirname(__file__))), 
                    'sample-documents'
                )
                
                if not os.path.exists(documents_folder):
                    logging.error(f"Documents folder not found: {documents_folder}")
                    return
                
                logging.info(f"Starting document processing for folder: {documents_folder}")
                processed_count, error_count = self.document_processor.process_all_documents(documents_folder)
                
                logging.info(f"Document processing completed: {processed_count} processed, {error_count} errors")
                
        except Exception as e:
            logging.error(f"Error in scheduled document processing: {str(e)}")
    
    def cleanup_old_devices(self):
        """Clean up devices that haven't been active for more than 30 days"""
        try:
            with self.app.app_context():
                from models.device import Device
                from datetime import timedelta
                
                # Find devices inactive for more than 30 days
                cutoff_date = datetime.utcnow() - timedelta(days=30)
                
                old_devices = Device.query.filter(
                    Device.last_seen < cutoff_date,
                    Device.is_active == True
                ).all()
                
                for device in old_devices:
                    device.is_active = False
                    logging.info(f"Deactivated old device: {device.device_name} (ID: {device.id})")
                
                if old_devices:
                    db.session.commit()
                    logging.info(f"Cleaned up {len(old_devices)} old devices")
                
        except Exception as e:
            logging.error(f"Error in device cleanup: {str(e)}")
    
    def trigger_manual_processing(self):
        """Manually trigger document processing"""
        try:
            with self.app.app_context():
                self.process_documents()
                return True
        except Exception as e:
            logging.error(f"Error in manual document processing: {str(e)}")
            return False

# Global scheduler instance
document_scheduler = DocumentScheduler()