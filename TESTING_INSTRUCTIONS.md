# 🧪 Juri-IQ Testing Instructions - Ready to Go!

## 🎉 System Status: ✅ FULLY OPERATIONAL

Both backend and frontend are now running and ready for testing!

---

## 🌐 Access URLs

### Frontend (Main Application)
- **Local**: http://localhost:3000
- **Public**: https://3000-e446688f-6183-4216-8c14-c55df0457436.proxy.daytona.works

### Backend API
- **Local**: http://localhost:5001
- **Swagger Docs**: http://localhost:5001/swagger

---

## 🔑 Login Credentials

### Admin Account (Recommended for Testing)
- **Email**: admin@test.com
- **Password**: Pass!2345
- **Access**: Full admin privileges

### Test User (You can create more)
- Use registration page to create new users
- All registered users get basic access

---

## 🚀 Quick Start Testing

### Step 1: Access the Frontend
1. Open your browser and go to:
   **https://3000-e446688f-6183-4216-8c14-c55df0457436.proxy.daytona.works**

2. You'll see the JurisIQ animation page
3. Click through to access the main application

### Step 2: Login as Admin
1. Navigate to the login page
2. Enter admin credentials:
   - Email: admin@test.com
   - Password: Pass!2345
3. Click "Login"

### Step 3: Explore the Dashboard
After successful login, you should see:
- User profile information
- Navigation menu
- Search functionality
- Document management options

### Step 4: Test Document Features
1. **Search Documents**: Use the search bar to find documents
2. **View Document Details**: Click on any search result
3. **Test Bookmarks**: Save documents to your bookmarks
4. **Admin Functions** (if logged in as admin):
   - Access user management
   - View system statistics
   - Manage user subscriptions

---

## 🧪 Feature Testing Checklist

### ✅ Authentication Testing
- [ ] Admin login works
- [ ] User registration works
- [ ] Token authentication works
- [ ] Logout functionality works
- [ ] Session management works

### ✅ Document Management Testing
- [ ] Document search returns results
- [ ] Document details display correctly
- [ ] Document bookmarking works
- [ ] Search history is maintained
- [ ] Filter options work

### ✅ Admin Panel Testing (Admin Account)
- [ ] User list displays correctly
- [ ] User statistics are accurate
- [ ] User management functions work
- [ ] Subscription management works

### ✅ UI/UX Testing
- [ ] All pages load without errors
- [ ] Navigation is smooth
- [ ] Responsive design works
- [ ] Error messages display correctly
- [ ] Loading states work

---

## 🔧 Backend Testing (Optional)

If you want to test the backend API directly:

### Using Swagger UI
1. Go to: http://localhost:5001/swagger
2. Test any endpoint directly in your browser
3. Use the admin credentials for authentication

### Using Test Script
```bash
cd juri-iq-x
./test_backend.sh
```

### Manual API Testing
```bash
# Health Check
curl http://localhost:5001/api/health

# Admin Login
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@test.com",
    "password": "Pass!2345",
    "deviceId": "test-device",
    "deviceName": "Test Device",
    "deviceType": "Web"
  }'
```

---

## 🐛 Troubleshooting

### Issue: Frontend Won't Load
**Solution**: 
- Check if proxy server is running: `ps aux | grep simple-proxy`
- Restart if needed: `cd juri-iq-x/webapp && python3 simple-proxy.py`

### Issue: API Calls Fail
**Solution**:
- Check if backend is running: `ps aux | grep dotnet`
- Verify backend URL: http://localhost:5001/api/health
- Check browser console for errors

### Issue: Login Fails
**Solution**:
- Verify credentials: admin@test.com / Pass!2345
- Check browser network tab for API responses
- Try refreshing the page

### Issue: CORS Errors
**Solution**:
- The proxy server handles CORS automatically
- Make sure you're accessing via the proxy URL, not directly

---

## 📱 Testing on Different Devices

The system is accessible from any device with internet access:

1. **Desktop**: Use the public URL in any browser
2. **Mobile**: Open the URL on your phone/tablet
3. **Tablet**: Test responsive design
4. **Different Browsers**: Test Chrome, Firefox, Safari, Edge

---

## 📊 Performance Testing

### Basic Performance Checks
1. **Page Load Times**: All pages should load within 2-3 seconds
2. **Search Response**: Search results should appear within 1-2 seconds
3. **API Response Times**: All API calls should respond within 500ms

### Stress Testing
- Try multiple simultaneous searches
- Test with multiple users logged in
- Check memory usage on the backend

---

## 🎯 Expected Results

### What Should Work
✅ Smooth login flow
✅ Document search and filtering
✅ Document detail views
✅ Bookmark management
✅ Admin panel access (for admin users)
✅ Responsive design
✅ Error handling

### Known Limitations
- Sample documents are basic text files
- AI processing is simulated for testing
- Some advanced features may be mock implementations

---

## 📝 Feedback Collection

While testing, note:
1. **Bugs**: Any errors or unexpected behavior
2. **Performance**: Slow loading or response times
3. **UX Issues**: Confusing navigation or workflows
4. **Missing Features**: Functionality you expected but didn't find

---

## 🚀 Ready to Test!

The system is fully operational and ready for comprehensive testing. Start with the basic authentication flow, then explore all the features systematically.

**Happy Testing! 🎉**

---

## 📞 Support

If you encounter any issues:
1. Check the troubleshooting section above
2. Look at browser console for JavaScript errors
3. Check network tab for failed API calls
4. Review backend logs for server-side errors

All services are monitored and should be running smoothly. Enjoy testing the Juri-IQ platform!