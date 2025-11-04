# Models package initialization
from .database import db, init_db
from .user import User
from .subscription import Subscription
from .device import Device
from .document import Document, DocumentContent
from .search_history import SearchHistory
from .bookmark import Bookmark

__all__ = [
    'db', 'init_db',
    'User', 'Subscription', 'Device',
    'Document', 'DocumentContent',
    'SearchHistory', 'Bookmark'
]