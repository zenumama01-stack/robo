"""
API Handler for Repo1
Provides API endpoint handling and communication
"""

import requests
import json
from typing import Dict, Any
from datetime import datetime

class APIHandler:
    """Handle API requests and responses"""
    
    def __init__(self):
        self.base_url = 'http://localhost:5000'
        self.timeout = 10
        self.request_log = []
    
    def test_endpoint(self, endpoint: str, method: str = 'GET', body: Dict = None) -> Dict:
        """Test API endpoint"""
        try:
            url = f"{self.base_url}{endpoint}"
            headers = {'Content-Type': 'application/json'}
            
            # Prepare request kwargs
            kwargs = {
                'headers': headers,
                'timeout': self.timeout
            }
            
            if body and method in ['POST', 'PUT', 'PATCH']:
                kwargs['json'] = body
            
            # Make request
            if method == 'GET':
                response = requests.get(url, **kwargs)
            elif method == 'POST':
                response = requests.post(url, **kwargs)
            elif method == 'PUT':
                response = requests.put(url, **kwargs)
            elif method == 'DELETE':
                response = requests.delete(url, **kwargs)
            elif method == 'PATCH':
                response = requests.patch(url, **kwargs)
            else:
                return {
                    'success': False,
                    'error': 'Unsupported HTTP method'
                }
            
            # Log request
            self.log_request(endpoint, method, response.status_code)
            
            # Parse response
            try:
                response_data = response.json()
            except:
                response_data = response.text
            
            return {
                'success': response.status_code < 400,
                'status_code': response.status_code,
                'endpoint': endpoint,
                'method': method,
                'response': response_data,
                'timestamp': datetime.now().isoformat()
            }
        
        except requests.exceptions.Timeout:
            return {
                'success': False,
                'error': 'Request timeout',
                'endpoint': endpoint,
                'method': method
            }
        except requests.exceptions.ConnectionError:
            return {
                'success': False,
                'error': 'Connection error',
                'endpoint': endpoint,
                'method': method
            }
        except Exception as e:
            return {
                'success': False,
                'error': str(e),
                'endpoint': endpoint,
                'method': method
            }
    
    def log_request(self, endpoint: str, method: str, status_code: int):
        """Log API request"""
        log_entry = {
            'timestamp': datetime.now().isoformat(),
            'endpoint': endpoint,
            'method': method,
            'status_code': status_code
        }
        self.request_log.append(log_entry)
        
        # Keep only last 100 requests
        if len(self.request_log) > 100:
            self.request_log.pop(0)
    
    def get_request_log(self) -> list:
        """Get request log"""
        return self.request_log
    
    def clear_request_log(self):
        """Clear request log"""
        self.request_log = []
    
    def validate_endpoint(self, endpoint: str) -> bool:
        """Validate endpoint format"""
        return endpoint.startswith('/')
