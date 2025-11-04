// ============================================
// API SERVICE - Centralized API Communication
// ============================================

/**
 * API Service for JurisIQ Application
 * Handles all communication with the backend API
 */

class ApiService {
    constructor() {
        // API Base URL - uses relative path since nginx proxies /api to backend
        this.baseURL = '/api';
        
        // Auth token management
        this.tokenKey = 'authToken';
    }

    /**
     * Get authentication token from localStorage
     */
    getToken() {
        return localStorage.getItem(this.tokenKey);
    }

    /**
     * Set authentication token in localStorage
     */
    setToken(token) {
        localStorage.setItem(this.tokenKey, token);
    }

    /**
     * Remove authentication token from localStorage
     */
    removeToken() {
        localStorage.removeItem(this.tokenKey);
    }

    /**
     * Check if user is authenticated
     */
    isAuthenticated() {
        return !!this.getToken();
    }

    /**
     * Get default headers for API requests
     */
    getHeaders(includeAuth = true) {
        const headers = {
            'Content-Type': 'application/json'
        };

        if (includeAuth) {
            const token = this.getToken();
            if (token) {
                headers['Authorization'] = `Bearer ${token}`;
            }
        }

        return headers;
    }

    /**
     * Make API request with error handling
     */
    async request(endpoint, options = {}) {
        const url = `${this.baseURL}${endpoint}`;
        
        try {
            const response = await fetch(url, {
                ...options,
                headers: {
                    ...this.getHeaders(options.includeAuth !== false),
                    ...options.headers
                }
            });

            // Handle 401 Unauthorized - redirect to login
            if (response.status === 401) {
                this.removeToken();
                window.location.href = 'login.html';
                throw new Error('Unauthorized - Please login again');
            }

            // Parse response
            const data = await response.json();

            // Handle non-OK responses
            if (!response.ok) {
                throw new Error(data.message || `API Error: ${response.status}`);
            }

            return data;
        } catch (error) {
            console.error('API Request Error:', error);
            throw error;
        }
    }

    /**
     * GET request
     */
    async get(endpoint, params = {}) {
        const queryString = new URLSearchParams(params).toString();
        const url = queryString ? `${endpoint}?${queryString}` : endpoint;
        
        return this.request(url, {
            method: 'GET'
        });
    }

    /**
     * POST request
     */
    async post(endpoint, data = {}, includeAuth = true) {
        return this.request(endpoint, {
            method: 'POST',
            body: JSON.stringify(data),
            includeAuth
        });
    }

    /**
     * PUT request
     */
    async put(endpoint, data = {}) {
        return this.request(endpoint, {
            method: 'PUT',
            body: JSON.stringify(data)
        });
    }

    /**
     * DELETE request
     */
    async delete(endpoint) {
        return this.request(endpoint, {
            method: 'DELETE'
        });
    }

    // ============================================
    // AUTH ENDPOINTS
    // ============================================

    /**
     * User login
     */
    async login(email, password, deviceId, deviceName = 'Web Browser', deviceType = 'Web') {
        const data = await this.post('/auth/login', {
            email,
            password,
            deviceId,
            deviceName,
            deviceType
        }, false);

        if (data.token) {
            this.setToken(data.token);
        }

        return data;
    }

    /**
     * User registration
     */
    async register(email, password, firstName, lastName) {
        return this.post('/auth/register', {
            email,
            password,
            firstName,
            lastName
        }, false);
    }

    /**
     * Logout user
     */
    logout() {
        this.removeToken();
        window.location.href = 'login.html';
    }

    // ============================================
    // DOCUMENT ENDPOINTS
    // ============================================

    /**
     * Search documents
     */
    async searchDocuments(query, filters = {}) {
        const params = {
            query,
            page: filters.page || 1,
            pageSize: filters.pageSize || 20
        };

        // Add optional filters
        if (filters.documentType) params.documentType = filters.documentType;
        if (filters.courtName) params.courtName = filters.courtName;
        if (filters.dateFrom) params.dateFrom = filters.dateFrom;
        if (filters.dateTo) params.dateTo = filters.dateTo;

        return this.get('/documents/search', params);
    }

    /**
     * Get document by ID
     */
    async getDocument(id) {
        return this.get(`/documents/${id}`);
    }

    /**
     * Get document statistics (views, bookmarks)
     */
    async getDocumentStatistics(id) {
        return this.get(`/documents/${id}/statistics`);
    }

    /**
     * Generate AI summary for document
     */
    async generateDocumentSummary(id) {
        return this.post(`/documents/${id}/generate-summary`, {});
    }

    /**
     * Get related documents
     */
    async getRelatedDocuments(id, limit = 5) {
        return this.get(`/documents/${id}/related?limit=${limit}`);
    }

    /**
     * Bookmark a document
     */
    async bookmarkDocument(id, notes = '') {
        return this.post(`/documents/${id}/bookmark`, { notes });
    }

    /**
     * Remove bookmark from document
     */
    async removeBookmark(id) {
        return this.delete(`/documents/${id}/bookmark`);
    }

    /**
     * Get user's bookmarks
     */
    async getBookmarks() {
        return this.get('/documents/bookmarks');
    }

    // ============================================
    // HEALTH CHECK
    // ============================================

    /**
     * Check API health
     */
    async healthCheck() {
        return this.get('/health');
    }
}

// Create singleton instance
const apiService = new ApiService();

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = apiService;
}