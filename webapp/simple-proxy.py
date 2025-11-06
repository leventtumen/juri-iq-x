#!/usr/bin/env python3
"""
Simple Proxy Server for Juri-IQ Frontend Testing
Proxies /api requests to localhost:5001 (backend)
Serves static files from current directory
"""

from http.server import HTTPServer, BaseHTTPRequestHandler
import urllib.request
import urllib.error
import json
import os

class ProxyHandler(BaseHTTPRequestHandler):
    def do_GET(self):
        if self.path.startswith('/api/'):
            self.proxy_request()
        else:
            self.serve_static_file()
    
    def do_POST(self):
        if self.path.startswith('/api/'):
            self.proxy_request()
        else:
            self.send_error(404, "Not Found")
    
    def do_PUT(self):
        if self.path.startswith('/api/'):
            self.proxy_request()
        else:
            self.send_error(404, "Not Found")
    
    def do_DELETE(self):
        if self.path.startswith('/api/'):
            self.proxy_request()
        else:
            self.send_error(404, "Not Found")
    
    def proxy_request(self):
        """Proxy request to backend server"""
        try:
            # Backend URL
            backend_url = f'http://localhost:5001{self.path}'
            
            # Prepare request data
            content_length = int(self.headers.get('Content-Length', 0))
            request_data = self.rfile.read(content_length) if content_length > 0 else None
            
            # Create request
            req = urllib.request.Request(backend_url, data=request_data, method=self.command)
            
            # Copy headers
            for header, value in self.headers.items():
                if header.lower() not in ['host', 'connection']:
                    req.add_header(header, value)
            
            # Make request to backend
            with urllib.request.urlopen(req) as response:
                # Send response
                self.send_response(response.getcode())
                
                # Copy headers
                for header, value in response.headers.items():
                    if header.lower() not in ['connection', 'transfer-encoding']:
                        self.send_header(header, value)
                
                self.end_headers()
                
                # Copy response body
                self.wfile.write(response.read())
                
        except urllib.error.HTTPError as e:
            self.send_response(e.code)
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            error_response = json.dumps({
                'success': False,
                'message': f'Backend error: {e.reason}',
                'errors': [str(e)]
            }).encode()
            self.wfile.write(error_response)
            
        except Exception as e:
            self.send_response(500)
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            error_response = json.dumps({
                'success': False,
                'message': f'Proxy error: {str(e)}',
                'errors': [str(e)]
            }).encode()
            self.wfile.write(error_response)
    
    def serve_static_file(self):
        """Serve static files"""
        # Default to index.html for root path
        if self.path == '/':
            self.path = '/index.html'
        
        # Security: don't allow directory traversal
        if '..' in self.path:
            self.send_error(403, "Forbidden")
            return
        
        # Try to find file
        file_path = self.path.lstrip('/')
        
        if not os.path.exists(file_path):
            # Try common HTML files
            if self.path.endswith('/'):
                file_path = self.path.lstrip('/') + 'index.html'
            elif not self.path.endswith('.html') and not self.path.endswith('.js') and not self.path.endswith('.css'):
                file_path = self.path.lstrip('/') + '.html'
        
        try:
            if os.path.exists(file_path) and os.path.isfile(file_path):
                # Determine content type
                content_type = self.get_content_type(file_path)
                
                # Read and serve file
                with open(file_path, 'rb') as f:
                    content = f.read()
                
                self.send_response(200)
                self.send_header('Content-Type', content_type)
                self.send_header('Content-Length', str(len(content)))
                self.end_headers()
                self.wfile.write(content)
            else:
                self.send_error(404, f"File not found: {file_path}")
                
        except Exception as e:
            self.send_error(500, f"Server error: {str(e)}")
    
    def get_content_type(self, file_path):
        """Get MIME type for file"""
        if file_path.endswith('.html'):
            return 'text/html'
        elif file_path.endswith('.css'):
            return 'text/css'
        elif file_path.endswith('.js'):
            return 'application/javascript'
        elif file_path.endswith('.json'):
            return 'application/json'
        elif file_path.endswith('.png'):
            return 'image/png'
        elif file_path.endswith('.jpg') or file_path.endswith('.jpeg'):
            return 'image/jpeg'
        elif file_path.endswith('.gif'):
            return 'image/gif'
        elif file_path.endswith('.svg'):
            return 'image/svg+xml'
        else:
            return 'text/plain'
    
    def log_message(self, format, *args):
        """Custom logging to suppress default messages"""
        print(f"📡 {self.address_string()} - {self.command} {self.path} - {format%args}")

if __name__ == '__main__':
    port = 3000
    server = HTTPServer(('0.0.0.0', port), ProxyHandler)
    
    print(f"🚀 Juri-IQ Proxy Server Starting...")
    print(f"📍 Frontend URL: http://localhost:{port}")
    print(f"🔗 API Proxy: http://localhost:{port}/api/* → http://localhost:5001/api/*")
    print(f"🌐 Public URL: https://3000-e446688f-6183-4216-8c14-c55df0457436.proxy.daytona.works")
    print(f"⏹️  Press Ctrl+C to stop")
    print("=" * 60)
    
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\n⏹️  Server stopped")
        server.shutdown()