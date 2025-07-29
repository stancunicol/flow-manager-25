window.cookieInterop = {
    // Override fetch to include credentials for all requests
    setupCredentials: () => {
        const originalFetch = window.fetch;
        window.fetch = function(input, init = {}) {
            init.credentials = 'include';
            console.log('[Cookie] Fetch intercepted:', input, init);
            return originalFetch(input, init);
        };
    },
    
    // Log cookies for debugging
    logCookies: () => {
        console.log('[Cookie] Current cookies:', document.cookie);
        return document.cookie;
    }
};