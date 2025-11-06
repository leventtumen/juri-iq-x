# 🧪 Complete Testing Guide for Juri-IQ Backend & Frontend

## 📋 Testing Overview

This guide provides comprehensive testing instructions for both the backend API and frontend integration of the Juri-IQ legal document processing platform.

---

## 🔧 Backend Testing

### Current Status
- **Backend URL**: http://localhost:5001
- **Status**: ✅ Running (Process ID: 5360)
- **API Documentation**: http://localhost:5001/swagger

### 1. API Documentation Testing (Swagger UI)

**Access Swagger UI:**
1. Open browser: http://localhost:5001/swagger
2. Interactive API documentation available
3. Test all endpoints directly from browser

### 2. Authentication Endpoints Testing

#### User Registration
```bash
curl -X POST "http://localhost:5001/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "TestPassword123!",
    "firstName": "Test",
    "lastName": "User"
  }'
```

#### User Login
```bash
curl -X POST "http://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "TestPassword123!",
    "deviceId": "test-device-123",
    "deviceName": "Test Device",
    "deviceType": "Web"
  }'
```

#### Admin Login (Pre-configured)
```bash
curl -X POST "http://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@test.com",
    "password": "Pass!2345",
    "deviceId": "admin-device-123",
    "deviceName": "Admin Device",
    "deviceType": "Web"
  }'
```

### 3. Document Processing Testing

#### Upload Document
```bash
curl -X POST "http://localhost:5001/api/documents/upload" \
  -H "Content-Type: multipart/form-data" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@sample-documents/sample_case.txt" \
  -F "title=Test Case Document" \
  -F "description=Test document for processing"
```

#### Get All Documents
```bash
curl -X GET "http://localhost:5001/api/documents" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### Process Document with AI
```bash
curl -X POST "http://localhost:5001/api/documents/{documentId}/process" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### 4. Admin Panel Testing

#### Get All Users (Admin Only)
```bash
curl -X GET "http://localhost:5001/api/admin/users" \
  -H "Authorization: Bearer ADMIN_JWT_TOKEN"
```

#### Get System Statistics (Admin Only)
```bash
curl -X GET "http://localhost:5001/api/admin/stats" \
  -H "Authorization: Bearer ADMIN_JWT_TOKEN"
```

### 5. Health Check
```bash
curl -X GET "http://localhost:5001/api/health"
```

---

## 🌐 Frontend Testing

### 1. Setup Local Web Server

Since the frontend expects `/api` to be proxied to the backend, you need to run a local web server:

#### Option A: Python HTTP Server
```bash
cd juri-iq-x/webapp
python3 -m http.server 8080
```

#### Option B: Node.js HTTP Server
```bash
cd juri-iq-x/webapp
npx serve -s . -l 8080
```

### 2. Frontend URLs to Test

1. **Main Entry Point**: http://localhost:8080 (redirects to animation)
2. **Animation Page**: http://localhost:8080/jurisiq-animation.html
3. **Dashboard**: http://localhost:8080/dashboard.html
4. **Login**: http://localhost:8080/login.html
5. **Search**: http://localhost:8080/search.html
6. **Case Detail**: http://localhost:8080/case-detail.html

### 3. Browser Testing Steps

#### Step 1: Test Authentication Flow
1. Navigate to: http://localhost:8080/login.html
2. Try admin login:
   - Email: admin@test.com
   - Password: Pass!2345
3. Verify successful login and redirect to dashboard

#### Step 2: Test Dashboard
1. After login, navigate to: http://localhost:8080/dashboard.html
2. Check user profile display
3. Test navigation to search functionality

#### Step 3: Test Document Search
1. Navigate to: http://localhost:8080/search.html
2. Enter search queries
3. Verify search results display
4. Test filter options

#### Step 4: Test Document Details
1. Click on any search result
2. Navigate to: http://localhost:8080/case-detail.html
3. Verify document details display
4. Test bookmark functionality

---

## 🔗 Full Integration Testing

### 1. Setup with Proxy (Recommended)

Create an nginx configuration to proxy `/api` requests to the backend:

#### Option A: Using Docker Compose (If Available)
```yaml
version: '3.8'
services:
  frontend:
    image: nginx:alpine
    ports:
      - "80:80"
    volumes:
      - ./webapp:/usr/share/nginx/html
      - ./nginx.conf:/etc/nginx/nginx.conf
  
  backend:
    build: ./backend
    ports:
      - "5001:5001"
```

#### Option B: Simple Python Proxy
```python
# proxy_server.py
from http.server import HTTPServer, BaseHTTPRequestHandler
import urllib.request
import json

class ProxyHandler(BaseHTTPRequestHandler):
    def do_GET(self):
        if self.path.startswith('/api/'):
            # Proxy to backend
            backend_url = f'http://localhost:5001{self.path}'
            try:
                response = urllib.request.urlopen(backend_url)
                self.send_response(response.getcode())
                for header, value in response.headers.items():
                    self.send_header(header, value)
                self.end_headers()
                self.wfile.write(response.read())
            except Exception as e:
                self.send_error(500, str(e))
        else:
            # Serve static files
            super().do_GET()

if __name__ == '__main__':
    server = HTTPServer(('localhost', 8080), ProxyHandler)
    server.serve_forever()
```

### 2. End-to-End Testing Script

```javascript
// test_e2e.js - Run in browser console
const testE2E = async () => {
    // Test 1: Health Check
    try {
        const health = await fetch('/api/health');
        console.log('✅ Health Check:', await health.json());
    } catch (error) {
        console.error('❌ Health Check Failed:', error);
    }

    // Test 2: Admin Login
    try {
        const loginResponse = await fetch('/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                email: 'admin@test.com',
                password: 'Pass!2345',
                deviceId: 'test-device',
                deviceName: 'Test Device',
                deviceType: 'Web'
            })
        });
        const loginData = await loginResponse.json();
        console.log('✅ Admin Login:', loginData);
        
        if (loginData.token) {
            localStorage.setItem('authToken', loginData.token);
        }
    } catch (error) {
        console.error('❌ Admin Login Failed:', error);
    }

    // Test 3: Get Documents
    try {
        const documents = await fetch('/api/documents', {
            headers: { 'Authorization': `Bearer ${localStorage.getItem('authToken')}` }
        });
        console.log('✅ Documents:', await documents.json());
    } catch (error) {
        console.error('❌ Get Documents Failed:', error);
    }
};

// Run tests
testE2E();
```

---

## 🐛 Common Issues & Solutions

### Issue 1: CORS Errors
**Solution**: Backend CORS is configured for development. Ensure frontend is running on localhost.

### Issue 2: Backend Not Responding
**Solution**: Check if backend is running:
```bash
ps aux | grep dotnet
# If not running, start it:
cd juri-iq-x/backend
dotnet run --urls="http://0.0.0.0:5001"
```

### Issue 3: Authentication Failures
**Solution**: Verify:
1. User exists in database
2. Password is correct
3. JWT token is properly stored

### Issue 4: Database Issues
**Solution**: Recreate database:
```bash
cd juri-iq-x/backend
rm jurisiq.db
dotnet run
```

---

## 📊 Testing Checklist

### Backend Tests
- [ ] API accessible at http://localhost:5001
- [ ] Swagger UI loads correctly
- [ ] Admin login works
- [ ] User registration works
- [ ] Document upload works
- [ ] AI processing works
- [ ] Admin endpoints accessible
- [ ] Health check passes

### Frontend Tests
- [ ] Web server starts on port 8080
- [ ] Login page loads
- [ ] Authentication flow works
- [ ] Dashboard displays correctly
- [ ] Search functionality works
- [ ] Document details display
- [ ] Navigation works smoothly

### Integration Tests
- [ ] Frontend can communicate with backend
- [ ] Authentication tokens are handled correctly
- [ ] API calls work from frontend
- [ ] Error handling works
- [ ] User experience is smooth

---

## 🚀 Quick Start Testing

1. **Start Backend** (already running): http://localhost:5001
2. **Start Frontend**: `cd juri-iq-x/webapp && python3 -m http.server 8080`
3. **Open Browser**: http://localhost:8080
4. **Login with Admin**: admin@test.com / Pass!2345
5. **Test Features**: Navigate through dashboard, search, and document views

The system is now ready for comprehensive testing! 🎉