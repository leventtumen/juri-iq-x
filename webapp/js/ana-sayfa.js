// ============================================
// ANA SAYFA JAVASCRIPT
// ============================================

// Check for authentication token
document.addEventListener('DOMContentLoaded', function() {
    // Check authentication using API service
    if (!apiService.isAuthenticated()) {
        // Redirect to login page if no auth token found
        window.location.href = 'login.html';
        return;
    }
    // Search Input
    const searchInput = document.querySelector('.search-input-main');
    const searchForm = document.querySelector('.search-form-main');
    
    // Voice Search Button
    const voiceBtn = document.querySelector('.voice-btn-main');
    if (voiceBtn) {
        voiceBtn.addEventListener('click', function() {
            if ('webkitSpeechRecognition' in window || 'SpeechRecognition' in window) {
                const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
                const recognition = new SpeechRecognition();
                
                recognition.lang = 'tr-TR';
                recognition.continuous = false;
                recognition.interimResults = false;
                
                recognition.onstart = function() {
                    voiceBtn.style.color = 'var(--danger)';
                    voiceBtn.innerHTML = '<i class="fas fa-microphone-slash"></i>';
                };
                
                recognition.onresult = function(event) {
                    const transcript = event.results[0][0].transcript;
                    searchInput.value = transcript;
                };
                
                recognition.onerror = function(event) {
                    console.error('Speech recognition error:', event.error);
                };
                
                recognition.onend = function() {
                    voiceBtn.style.color = '';
                    voiceBtn.innerHTML = '<i class="fas fa-microphone"></i>';
                };
                
                recognition.start();
            } else {
                alert('Tarayıcınız sesli aramayı desteklemiyor.');
            }
        });
    }
    
    // Filter Chips
    const filterChips = document.querySelectorAll('.filter-chip-main');
    filterChips.forEach(chip => {
        chip.addEventListener('click', function() {
            // Remove active from all
            filterChips.forEach(c => c.classList.remove('active'));
            // Add active to clicked
            this.classList.add('active');
            
            // Store filter preference
            const filter = this.dataset.filter;
            localStorage.setItem('searchFilter', filter);
        });
    });
    
    // Search Form Submit - Allow normal form submission
    if (searchForm) {
        searchForm.addEventListener('submit', function(e) {
            const query = searchInput.value.trim();
            
            if (!query) {
                e.preventDefault();
                searchInput.focus();
                searchInput.style.borderColor = 'var(--danger)';
                setTimeout(() => {
                    searchInput.style.borderColor = '';
                }, 2000);
                return;
            }
            
            // Track search
            trackSearch(query);
            
            // Allow normal form submission to search-results.html
            // The form action is already set to "search-results.html"
        });
    }
    
    // Popular Tags Click Tracking
    const popularTags = document.querySelectorAll('.popular-tag-main');
    popularTags.forEach(tag => {
        tag.addEventListener('click', function() {
            const query = this.textContent;
            trackSearch(query);
        });
    });
    
    // Enter key to search
    if (searchInput) {
        searchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                searchForm.dispatchEvent(new Event('submit'));
            }
        });
    }
});

// Track search in history
function trackSearch(query) {
    const searches = JSON.parse(localStorage.getItem('searchHistory') || '[]');
    searches.unshift({
        query: query,
        timestamp: new Date().toISOString(),
        filter: localStorage.getItem('searchFilter') || 'all'
    });
    
    // Keep only last 50 searches
    if (searches.length > 50) {
        searches.pop();
    }
    
    localStorage.setItem('searchHistory', JSON.stringify(searches));
}

// Show message notification
function showMessage(message, type = 'info') {
    const messageDiv = document.createElement('div');
    messageDiv.className = `message-notification message-${type}`;
    
    const icons = {
        success: 'fa-check-circle',
        error: 'fa-exclamation-circle',
        info: 'fa-info-circle',
        warning: 'fa-exclamation-triangle'
    };
    
    const colors = {
        success: 'var(--success)',
        error: 'var(--danger)',
        info: 'var(--info)',
        warning: 'var(--warning)'
    };
    
    messageDiv.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        padding: 1rem 1.5rem;
        border-radius: var(--border-radius-md);
        background: white;
        box-shadow: var(--shadow-lg);
        display: flex;
        align-items: center;
        gap: var(--spacing-md);
        border-left: 4px solid ${colors[type]};
        animation: slideIn 0.3s ease;
    `;
    
    messageDiv.innerHTML = `
        <i class="fas ${icons[type]}" style="color: ${colors[type]}; font-size: var(--fs-lg);"></i>
        <span style="color: var(--text-primary);">${message}</span>
    `;
    
    document.body.appendChild(messageDiv);
    
    setTimeout(() => {
        messageDiv.style.animation = 'slideOut 0.3s ease';
        setTimeout(() => messageDiv.remove(), 300);
    }, 3000);
}

// Add CSS animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOut {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);