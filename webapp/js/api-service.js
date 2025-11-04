class ApiService {
    constructor() {
        this.baseUrl = 'http://localhost:5000/api';
        this.token = localStorage.getItem('juris_iq_token');
    }

    // Utility methods
    setToken(token) {
        this.token = token;
        localStorage.setItem('juris_iq_token', token);
    }

    removeToken() {
        this.token = null;
        localStorage.removeItem('juris_iq_token');
    }

    async request(endpoint, options = {}) {
        const url = `${this.baseUrl}${endpoint}`;
        const config = {
            headers: {
                'Content-Type': 'application/json',
                ...options.headers
            },
            ...options
        };

        if (this.token) {
            config.headers['Authorization'] = `Bearer ${this.token}`;
        }

        try {
            const response = await fetch(url, config);
            const data = await response.json();

            // Handle token expiration
            if (data.redirect_to_login) {
                this.removeToken();
                window.location.href = 'login.html';
                return null;
            }

            if (!response.ok) {
                throw new Error(data.message || `HTTP error! status: ${response.status}`);
            }

            return data;
        } catch (error) {
            console.error('API request failed:', error);
            throw error;
        }
    }

    // Authentication endpoints
    async login(email, password) {
        const data = await this.request('/auth/login', {
            method: 'POST',
            body: JSON.stringify({ email, password })
        });
        
        if (data && data.access_token) {
            this.setToken(data.access_token);
            return data.user;
        }
        return null;
    }

    async register(email, password, subscriptionType = 'simple') {
        const data = await this.request('/auth/register', {
            method: 'POST',
            body: JSON.stringify({ 
                email, 
                password, 
                subscription_type: subscriptionType 
            })
        });
        
        if (data && data.access_token) {
            this.setToken(data.access_token);
            return data.user;
        }
        return null;
    }

    async logout() {
        try {
            await this.request('/auth/logout', {
                method: 'POST'
            });
        } catch (error) {
            console.error('Logout error:', error);
        } finally {
            this.removeToken();
        }
    }

    async getCurrentUser() {
        return await this.request('/auth/me');
    }

    async refreshToken() {
        const data = await this.request('/auth/refresh', {
            method: 'POST'
        });
        
        if (data && data.access_token) {
            this.setToken(data.access_token);
            return data.user;
        }
        return null;
    }

    // Document endpoints
    async getDocuments(page = 1, perPage = 20, fileType = null, processed = null) {
        let endpoint = `/documents/?page=${page}&per_page=${perPage}`;
        
        if (fileType) {
            endpoint += `&file_type=${fileType}`;
        }
        
        if (processed !== null) {
            endpoint += `&processed=${processed}`;
        }
        
        return await this.request(endpoint);
    }

    async getDocument(documentId) {
        return await this.request(`/documents/${documentId}`);
    }

    async searchDocuments(query, page = 1, perPage = 20, similarityThreshold = 0.1) {
        return await this.request('/search/query', {
            method: 'POST',
            body: JSON.stringify({
                query,
                page,
                per_page: perPage,
                similarity_threshold: similarityThreshold
            })
        });
    }

    async getDocumentStats() {
        return await this.request('/documents/stats');
    }

    async getDocumentTypes() {
        return await this.request('/documents/types');
    }

    async getSimilarDocuments(documentId, threshold = 0.2, limit = 10) {
        let endpoint = `/search/similar/${documentId}?threshold=${threshold}&limit=${limit}`;
        return await this.request(endpoint);
    }

    async getSearchSuggestions(query) {
        return await this.request(`/search/suggestions?q=${encodeURIComponent(query)}`);
    }

    // Search endpoints
    async getSearchHistory() {
        return await this.request('/search/history');
    }

    // Bookmark endpoints
    async createBookmark(documentId, notes = '') {
        return await this.request('/bookmarks/', {
            method: 'POST',
            body: JSON.stringify({
                document_id: documentId,
                notes
            })
        });
    }

    async getBookmarks(page = 1, perPage = 20) {
        return await this.request(`/bookmarks/?page=${page}&per_page=${perPage}`);
    }

    async getBookmark(bookmarkId) {
        return await this.request(`/bookmarks/${bookmarkId}`);
    }

    async updateBookmark(bookmarkId, notes) {
        return await this.request(`/bookmarks/${bookmarkId}`, {
            method: 'PUT',
            body: JSON.stringify({ notes })
        });
    }

    async deleteBookmark(bookmarkId) {
        return await this.request(`/bookmarks/${bookmarkId}`, {
            method: 'DELETE'
        });
    }

    async getBookmarkByDocument(documentId) {
        return await this.request(`/bookmarks/document/${documentId}`);
    }

    async getBookmarkStats() {
        return await this.request('/bookmarks/stats');
    }

    // Profile endpoints
    async getProfile() {
        return await this.request('/profile/');
    }

    async getSubscription() {
        return await this.request('/profile/subscription');
    }

    async updateSubscription(subscriptionType) {
        return await this.request('/profile/subscription', {
            method: 'PUT',
            body: JSON.stringify({
                subscription_type: subscriptionType
            })
        });
    }

    async getDevices() {
        return await this.request('/profile/devices');
    }

    async removeDevice(deviceId) {
        return await this.request(`/profile/devices/${deviceId}`, {
            method: 'DELETE'
        });
    }

    async updateDeviceName(deviceId, deviceName) {
        return await this.request(`/profile/devices/${deviceId}`, {
            method: 'PUT',
            body: JSON.stringify({
                device_name: deviceName
            })
        });
    }

    async getActivity(activityType = 'all', limit = 20) {
        return await this.request(`/profile/activity?type=${activityType}&limit=${limit}`);
    }

    async getProfileStats() {
        return await this.request('/profile/stats');
    }

    // Admin endpoints
    async getDashboard() {
        return await this.request('/admin/dashboard');
    }

    async getUsers(page = 1, perPage = 20, filters = {}) {
        let endpoint = `/admin/users?page=${page}&per_page=${perPage}`;
        
        if (filters.is_active !== undefined) {
            endpoint += `&is_active=${filters.is_active}`;
        }
        
        if (filters.is_admin !== undefined) {
            endpoint += `&is_admin=${filters.is_admin}`;
        }
        
        if (filters.is_blacklisted !== undefined) {
            endpoint += `&is_blacklisted=${filters.is_blacklisted}`;
        }
        
        if (filters.subscription_type) {
            endpoint += `&subscription_type=${filters.subscription_type}`;
        }
        
        return await this.request(endpoint);
    }

    async getUserDetails(userId) {
        return await this.request(`/admin/users/${userId}`);
    }

    async toggleUserStatus(userId, action) {
        return await this.request(`/admin/users/${userId}/toggle-status`, {
            method: 'POST',
            body: JSON.stringify({ action })
        });
    }

    async updateUserSubscription(userId, subscriptionType, isActive = true) {
        return await this.request(`/admin/users/${userId}/subscription`, {
            method: 'PUT',
            body: JSON.stringify({
                subscription_type: subscriptionType,
                is_active: isActive
            })
        });
    }

    async getUserDevices(userId) {
        return await this.request(`/admin/users/${userId}/devices`);
    }

    async revokeUserDevice(userId, deviceId) {
        return await this.request(`/admin/users/${userId}/devices/${deviceId}/revoke`, {
            method: 'POST'
        });
    }

    async triggerDocumentProcessing() {
        return await this.request('/admin/system/process-documents', {
            method: 'POST'
        });
    }

    async getSystemStats() {
        return await this.request('/admin/system/stats');
    }

    // Utility method to check if user is authenticated
    isAuthenticated() {
        return !!this.token;
    }

    // Utility method to handle API errors
    handleError(error, defaultMessage = 'An error occurred') {
        console.error('API Error:', error);
        const message = error.message || defaultMessage;
        
        // Show error message to user (you might want to integrate with a toast/notification system)
        if (typeof window !== 'undefined' && window.alert) {
            alert(message);
        }
        
        return message;
    }
}

// Create global instance
const apiService = new ApiService();

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ApiService;
}

// Make available globally for browser usage
if (typeof window !== 'undefined') {
    window.apiService = apiService;
    window.ApiService = ApiService;
}