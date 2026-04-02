"""
Authentication Handler for Repo1
Provides user management and authentication functionality
"""

import hashlib
import json
from datetime import datetime
from typing import Dict, Any, List
import os

class AuthHandler:
    """Handle user authentication and management"""
    
    def __init__(self):
        self.users_file = 'data/users.json'
        self.ensure_data_dir()
        self.users = self.load_users()
    
    def ensure_data_dir(self):
        """Ensure data directory exists"""
        os.makedirs('data', exist_ok=True)
    
    def load_users(self) -> List[Dict]:
        """Load users from file"""
        try:
            if os.path.exists(self.users_file):
                with open(self.users_file, 'r') as f:
                    return json.load(f)
        except Exception as e:
            print(f"Error loading users: {e}")
        return []
    
    def save_users(self):
        """Save users to file"""
        try:
            with open(self.users_file, 'w') as f:
                json.dump(self.users, f, indent=2)
        except Exception as e:
            print(f"Error saving users: {e}")
    
    def hash_password(self, password: str) -> str:
        """Hash password using SHA-256"""
        return hashlib.sha256(password.encode()).hexdigest()
    
    def register_user(self, email: str, password: str, name: str, role: str = 'user') -> Dict:
        """Register a new user"""
        # Check if user already exists
        if any(u['email'] == email for u in self.users):
            return {
                'success': False,
                'error': 'User already exists',
                'message': f'User with email {email} already registered'
            }
        
        user_id = len(self.users) + 1
        user = {
            'id': user_id,
            'email': email,
            'password_hash': self.hash_password(password),
            'name': name,
            'role': role,
            'created_at': datetime.now().isoformat(),
            'is_active': True
        }
        
        self.users.append(user)
        self.save_users()
        
        return {
            'success': True,
            'user_id': user_id,
            'email': email,
            'name': name,
            'message': 'User registered successfully'
        }
    
    def login_user(self, email: str, password: str) -> Dict:
        """Authenticate user login"""
        password_hash = self.hash_password(password)
        
        for user in self.users:
            if user['email'] == email and user['password_hash'] == password_hash:
                if user['is_active']:
                    return {
                        'success': True,
                        'user_id': user['id'],
                        'email': user['email'],
                        'name': user['name'],
                        'role': user['role'],
                        'message': 'Login successful'
                    }
                else:
                    return {
                        'success': False,
                        'error': 'User is inactive'
                    }
        
        return {
            'success': False,
            'error': 'Invalid credentials'
        }
    
    def get_user(self, user_id: int) -> Dict[str, Any]:
        """Get user by ID"""
        for user in self.users:
            if user['id'] == user_id:
                # Don't return password hash
                user_copy = user.copy()
                user_copy.pop('password_hash', None)
                return user_copy
        return None
    
    def get_user_by_email(self, email: str) -> Dict[str, Any]:
        """Get user by email"""
        for user in self.users:
            if user['email'] == email:
                user_copy = user.copy()
                user_copy.pop('password_hash', None)
                return user_copy
        return None
    
    def get_all_users(self) -> List[Dict]:
        """Get all users without password hashes"""
        return [{
            **u,
            'password_hash': '***'
        } for u in self.users]
    
    def update_user(self, user_id: int, data: Dict) -> Dict:
        """Update user information"""
        for user in self.users:
            if user['id'] == user_id:
                # Update allowed fields
                allowed_fields = ['name', 'role', 'is_active']
                for field in allowed_fields:
                    if field in data:
                        user[field] = data[field]
                
                user['updated_at'] = datetime.now().isoformat()
                self.save_users()
                
                return {
                    'success': True,
                    'message': 'User updated successfully',
                    'user': {**user, 'password_hash': '***'}
                }
        
        return {
            'success': False,
            'error': 'User not found'
        }
    
    def delete_user(self, user_id: int) -> Dict:
        """Delete user"""
        initial_count = len(self.users)
        self.users = [u for u in self.users if u['id'] != user_id]
        
        if len(self.users) < initial_count:
            self.save_users()
            return {
                'success': True,
                'message': 'User deleted successfully'
            }
        else:
            return {
                'success': False,
                'error': 'User not found'
            }
    
    def change_password(self, user_id: int, old_password: str, new_password: str) -> Dict:
        """Change user password"""
        old_hash = self.hash_password(old_password)
        
        for user in self.users:
            if user['id'] == user_id:
                if user['password_hash'] == old_hash:
                    user['password_hash'] = self.hash_password(new_password)
                    self.save_users()
                    return {
                        'success': True,
                        'message': 'Password changed successfully'
                    }
                else:
                    return {
                        'success': False,
                        'error': 'Old password is incorrect'
                    }
        
        return {
            'success': False,
            'error': 'User not found'
        }
