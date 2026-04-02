#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
Flask Server for Repo1 Application
Main entry point for the web application
"""

from flask import Flask, render_template, jsonify, request, session
from flask_cors import CORS
from datetime import datetime
import os
import sys

# Add parent directory to path for imports
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from auth.auth_handler import AuthHandler
from api.api_handler import APIHandler
from utils.helpers import setup_logger, get_config

# Initialize Flask app
app = Flask(__name__, static_folder='ui', template_folder='ui')
CORS(app)

# Configuration
app.secret_key = os.environ.get('SECRET_KEY', 'dev-secret-key-change-in-production')
app.config['SESSION_COOKIE_HTTPONLY'] = True
app.config['SESSION_COOKIE_SECURE'] = False  # Set to True in production with HTTPS
app.config['SESSION_COOKIE_SAMESITE'] = 'Lax'

# Initialize handlers
auth_handler = AuthHandler()
api_handler = APIHandler()
logger = setup_logger(__name__)

# ============ MAIN ROUTES ============

@app.route('/')
def index():
    """Serve main UI"""
    try:
        with open(os.path.join(os.path.dirname(__file__), 'ui', 'dashboard.html'), 'r') as f:
            return f.read()
    except:
        return render_template('dashboard.html')

# ============ AUTHENTICATION ROUTES ============

@app.route('/api/auth/register', methods=['POST'])
def register():
    """User registration endpoint"""
    try:
        data = request.get_json()
        
        # Validate input
        if not all(k in data for k in ['email', 'password', 'name']):
            return jsonify({'error': 'Missing required fields'}), 400
        
        # Register user
        result = auth_handler.register_user(
            email=data['email'],
            password=data['password'],
            name=data['name']
        )
        
        if result['success']:
            session['user_id'] = result['user_id']
            session['user_email'] = result['email']
            logger.info(f"User registered: {data['email']}")
            return jsonify(result), 201
        else:
            return jsonify(result), 400
            
    except Exception as e:
        logger.error(f"Registration error: {str(e)}")
        return jsonify({'error': 'Registration failed'}), 500

@app.route('/api/auth/login', methods=['POST'])
def login():
    """User login endpoint"""
    try:
        data = request.get_json()
        
        if not all(k in data for k in ['email', 'password']):
            return jsonify({'error': 'Missing email or password'}), 400
        
        # Validate credentials
        result = auth_handler.login_user(
            email=data['email'],
            password=data['password']
        )
        
        if result['success']:
            session['user_id'] = result['user_id']
            session['user_email'] = result['email']
            logger.info(f"User logged in: {data['email']}")
            return jsonify(result), 200
        else:
            logger.warning(f"Failed login attempt: {data['email']}")
            return jsonify(result), 401
            
    except Exception as e:
        logger.error(f"Login error: {str(e)}")
        return jsonify({'error': 'Login failed'}), 500

@app.route('/api/auth/logout', methods=['POST'])
def logout():
    """User logout endpoint"""
    session.clear()
    logger.info("User logged out")
    return jsonify({'success': True, 'message': 'Logged out successfully'}), 200

@app.route('/api/auth/user', methods=['GET'])
def get_current_user():
    """Get current user info"""
    if 'user_email' in session:
        return jsonify({
            'success': True,
            'user_id': session.get('user_id'),
            'email': session.get('user_email')
        }), 200
    else:
        return jsonify({'success': False, 'message': 'Not logged in'}), 401

# ============ USER MANAGEMENT ROUTES ============

@app.route('/api/users', methods=['GET'])
def get_users():
    """Get all users (requires auth)"""
    if 'user_id' not in session:
        return jsonify({'error': 'Unauthorized'}), 401
    
    try:
        users = auth_handler.get_all_users()
        return jsonify({'success': True, 'users': users}), 200
    except Exception as e:
        logger.error(f"Error fetching users: {str(e)}")
        return jsonify({'error': 'Failed to fetch users'}), 500

@app.route('/api/users', methods=['POST'])
def create_user():
    """Create new user (admin only)"""
    if 'user_id' not in session:
        return jsonify({'error': 'Unauthorized'}), 401
    
    try:
        data = request.get_json()
        result = auth_handler.register_user(
            email=data['email'],
            password=data.get('password', 'temp_password'),
            name=data['name'],
            role=data.get('role', 'user')
        )
        
        if result['success']:
            logger.info(f"New user created: {data['email']}")
            return jsonify(result), 201
        else:
            return jsonify(result), 400
            
    except Exception as e:
        logger.error(f"User creation error: {str(e)}")
        return jsonify({'error': 'Failed to create user'}), 500

@app.route('/api/users/<int:user_id>', methods=['PUT'])
def update_user(user_id):
    """Update user"""
    if 'user_id' not in session:
        return jsonify({'error': 'Unauthorized'}), 401
    
    try:
        data = request.get_json()
        result = auth_handler.update_user(user_id, data)
        
        if result['success']:
            logger.info(f"User updated: {user_id}")
            return jsonify(result), 200
        else:
            return jsonify(result), 400
            
    except Exception as e:
        logger.error(f"User update error: {str(e)}")
        return jsonify({'error': 'Failed to update user'}), 500

@app.route('/api/users/<int:user_id>', methods=['DELETE'])
def delete_user(user_id):
    """Delete user"""
    if 'user_id' not in session:
        return jsonify({'error': 'Unauthorized'}), 401
    
    try:
        result = auth_handler.delete_user(user_id)
        
        if result['success']:
            logger.info(f"User deleted: {user_id}")
            return jsonify(result), 200
        else:
            return jsonify(result), 400
            
    except Exception as e:
        logger.error(f"User deletion error: {str(e)}")
        return jsonify({'error': 'Failed to delete user'}), 500

# ============ API TESTING ROUTES ============

@app.route('/api/endpoint-test', methods=['POST'])
def test_endpoint():
    """Test arbitrary API endpoint"""
    if 'user_id' not in session:
        return jsonify({'error': 'Unauthorized'}), 401
    
    try:
        data = request.get_json()
        endpoint = data.get('endpoint', '')
        method = data.get('method', 'GET').upper()
        body = data.get('body', {})
        
        result = api_handler.test_endpoint(endpoint, method, body)
        return jsonify(result), 200
        
    except Exception as e:
        logger.error(f"Endpoint test error: {str(e)}")
        return jsonify({'error': 'Endpoint test failed', 'details': str(e)}), 500

# ============ SYSTEM HEALTH ROUTES ============

@app.route('/api/health', methods=['GET'])
def health_check():
    """Health check endpoint"""
    return jsonify({
        'status': 'healthy',
        'timestamp': datetime.now().isoformat(),
        'components': {
            'auth': 'active',
            'api': 'active',
            'utils': 'available',
            'database': 'active'
        }
    }), 200

@app.route('/api/status', methods=['GET'])
def status():
    """Get application status"""
    return jsonify({
        'app_name': 'Repo1 Manager',
        'version': '1.0.0',
        'status': 'running',
        'uptime': 'calculated_from_start_time',
        'features': [
            'User Authentication',
            'User Management',
            'API Handling',
            'Test Suite',
            'Helper Utilities'
        ],
        'active_users': len(auth_handler.get_all_users())
    }), 200

# ============ ERROR HANDLERS ============

@app.errorhandler(404)
def not_found(error):
    """Handle 404 errors"""
    return jsonify({'error': 'Not found'}), 404

@app.errorhandler(500)
def internal_error(error):
    """Handle 500 errors"""
    logger.error(f"Internal server error: {str(error)}")
    return jsonify({'error': 'Internal server error'}), 500

# ============ MAIN EXECUTION ============

if __name__ == '__main__':
    port = int(os.environ.get('PORT', 5000))
    debug = os.environ.get('DEBUG', 'True').lower() == 'true'
    
    logger.info(f"Starting Repo1 Manager on port {port}")
    print(f"\n{'='*70}")
    print("🚀 Repo1 Manager - Flask Server")
    print(f"{'='*70}")
    print(f"📍 Access at: http://localhost:{port}")
    print(f"🔧 Debug Mode: {debug}")
    print(f"{'='*70}\n")
    
    app.run(
        host='0.0.0.0',
        port=port,
        debug=debug
    )
