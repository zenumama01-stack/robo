"""
Helper utilities for Repo1
Provides common utility functions and configurations
"""

import logging
import json
import os
import re
from datetime import datetime
from typing import Dict, Any, List, Tuple

# ============ LOGGING ============

def setup_logger(name: str) -> logging.Logger:
    """Setup logger for module"""
    logger = logging.getLogger(name)
    
    if not logger.handlers:
        os.makedirs('data', exist_ok=True)
        handler = logging.FileHandler('data/app.log')
        formatter = logging.Formatter(
            '%(asctime)s - %(name)s - %(levelname)s - %(message)s',
            datefmt='%Y-%m-%d %H:%M:%S'
        )
        handler.setFormatter(formatter)
        logger.addHandler(handler)
        logger.setLevel(logging.INFO)
    
    return logger

# ============ CONFIGURATION ============

def get_config() -> Dict[str, Any]:
    """Get application configuration"""
    return {
        'app_name': 'Repo1 Manager',
        'version': '1.0.0',
        'debug': os.environ.get('DEBUG', 'True').lower() == 'true',
        'port': int(os.environ.get('PORT', 5000)),
        'host': os.environ.get('HOST', '0.0.0.0'),
        'database_path': 'data/',
        'log_file': 'data/app.log',
        'session_timeout': 3600,
        'features': [
            'User Authentication',
            'User Management',
            'API Handling',
            'Health Monitoring',
            'Logging System'
        ]
    }

# ============ FILE I/O ============

def read_json_file(filepath: str) -> Dict:
    """Read JSON file"""
    try:
        if os.path.exists(filepath):
            with open(filepath, 'r', encoding='utf-8') as f:
                return json.load(f)
        return {}
    except Exception as e:
        logger = setup_logger(__name__)
        logger.error(f"Error reading JSON file {filepath}: {str(e)}")
        return {}

def write_json_file(filepath: str, data: Any) -> bool:
    """Write JSON file"""
    try:
        os.makedirs(os.path.dirname(filepath) or '.', exist_ok=True)
        with open(filepath, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2, ensure_ascii=False)
        return True
    except Exception as e:
        logger = setup_logger(__name__)
        logger.error(f"Error writing JSON file {filepath}: {str(e)}")
        return False

# ============ DATETIME UTILITIES ============

def get_timestamp() -> str:
    """Get current ISO timestamp"""
    return datetime.now().isoformat()

def parse_timestamp(ts: str) -> datetime:
    """Parse ISO timestamp"""
    try:
        return datetime.fromisoformat(ts)
    except:
        return None

# ============ VALIDATION ============

def validate_email(email: str) -> Dict[str, Any]:
    """Validate email format"""
    pattern = r'^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$'
    is_valid = bool(re.match(pattern, email))
    
    return {
        'valid': is_valid,
        'issues': [] if is_valid else ['Invalid email format']
    }

def validate_password(password: str) -> Dict[str, Any]:
    """Validate password strength"""
    issues = []
    strength = 'weak'
    
    if len(password) < 8:
        issues.append('Password must be at least 8 characters')
    if not any(c.isupper() for c in password):
        issues.append('Password must contain uppercase letter')
    if not any(c.isdigit() for c in password):
        issues.append('Password must contain digit')
    if not any(c in '!@#$%^&*()_+-=[]{}|;:,.<>?' for c in password):
        issues.append('Password must contain special character')
    
    if not issues:
        strength = 'strong'
    elif len(issues) <= 2:
        strength = 'medium'
    
    return {
        'valid': len(issues) == 0,
        'issues': issues,
        'strength': strength
    }

# ============ STRING UTILITIES ============

def truncate_string(s: str, length: int = 50) -> str:
    """Truncate string to specified length"""
    if len(s) > length:
        return s[:length] + '...'
    return s

def capitalize_words(s: str) -> str:
    """Capitalize each word in string"""
    return ' '.join(word.capitalize() for word in s.split())

# ============ STATISTICS ============

class Statistics:
    """Application statistics tracker"""
    
    def __init__(self):
        self.requests = 0
        self.errors = 0
        self.start_time = datetime.now()
    
    def add_request(self):
        """Log a request"""
        self.requests += 1
    
    def add_error(self):
        """Log an error"""
        self.errors += 1
    
    def get_uptime(self) -> str:
        """Get uptime string"""
        delta = datetime.now() - self.start_time
        hours = delta.seconds // 3600
        minutes = (delta.seconds % 3600) // 60
        return f"{hours}h {minutes}m"
    
    def get_error_rate(self) -> float:
        """Get error rate as percentage"""
        if self.requests == 0:
            return 0.0
        return (self.errors / self.requests) * 100
    
    def get_stats(self) -> Dict[str, Any]:
        """Get all statistics"""
        return {
            'requests': self.requests,
            'errors': self.errors,
            'error_rate': f"{self.get_error_rate():.2f}%",
            'uptime': self.get_uptime(),
            'start_time': self.start_time.isoformat()
        }
