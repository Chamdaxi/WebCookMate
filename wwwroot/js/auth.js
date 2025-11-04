// Authentication and API utilities
class AuthService {
    constructor() {
        this.baseUrl = '';
        this.accessToken = localStorage.getItem('accessToken');
    }

    // Get access token from localStorage
    getAccessToken() {
        return localStorage.getItem('accessToken');
    }

    // Set access token in localStorage
    setAccessToken(token) {
        localStorage.setItem('accessToken', token);
        this.accessToken = token;
    }

    // Remove access token
    clearAccessToken() {
        localStorage.removeItem('accessToken');
        this.accessToken = null;
    }

    // Make API call with Bearer token
    async apiCall(url, options = {}) {
        const token = this.getAccessToken();
        
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
                ...(token && { 'Authorization': `Bearer ${token}` })
            }
        };

        const finalOptions = {
            ...defaultOptions,
            ...options,
            headers: {
                ...defaultOptions.headers,
                ...options.headers
            }
        };

        try {
            const response = await fetch(url, finalOptions);
            
            // If token is invalid, clear it
            if (response.status === 401) {
                this.clearAccessToken();
                window.location.href = '/Auth/Login';
                return null;
            }

            return response;
        } catch (error) {
            console.error('API call failed:', error);
            throw error;
        }
    }

    // Get user profile
    async getUserProfile() {
        const response = await this.apiCall('/api/user/profile');
        if (response && response.ok) {
            return await response.json();
        }
        return null;
    }

    // Get meal plans
    async getMealPlans() {
        const response = await this.apiCall('/api/meal-plans');
        if (response && response.ok) {
            return await response.json();
        }
        return null;
    }

    // Get recipes
    async getRecipes(category = null) {
        let url = '/api/recipes';
        if (category) {
            url += `?category=${category}`;
        }
        
        const response = await this.apiCall(url);
        if (response && response.ok) {
            return await response.json();
        }
        return null;
    }

    // Logout
    logout() {
        this.clearAccessToken();
        window.location.href = '/Auth/Logout';
    }
}

// Initialize global auth service
window.authService = new AuthService();

// Auto-setup Bearer token for all fetch requests
const originalFetch = window.fetch;
window.fetch = function(url, options = {}) {
    const token = window.authService.getAccessToken();
    
    if (token && !options.headers?.['Authorization']) {
        options.headers = {
            ...options.headers,
            'Authorization': `Bearer ${token}`
        };
    }
    
    return originalFetch(url, options);
};




