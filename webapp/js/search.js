// ============================================
// SEARCH PAGE JAVASCRIPT
// ============================================

document.addEventListener('DOMContentLoaded', function() {
    // Search Input
    const searchInput = document.querySelector('.search-input');
    const searchSuggestions = document.getElementById('searchSuggestions');
    
    if (searchInput && searchSuggestions) {
        // Show suggestions on focus
        searchInput.addEventListener('focus', function() {
            if (this.value.length > 0) {
                searchSuggestions.style.display = 'block';
            }
        });
        
        // Hide suggestions on blur (with delay for click)
        searchInput.addEventListener('blur', function() {
            setTimeout(() => {
                searchSuggestions.style.display = 'none';
            }, 200);
        });
        
        // Show suggestions on input
        searchInput.addEventListener('input', function() {
            if (this.value.length > 2) {
                searchSuggestions.style.display = 'block';
                // Here you would fetch suggestions from API
            } else {
                searchSuggestions.style.display = 'none';
            }
        });
        
        // Handle suggestion click
        const suggestionItems = document.querySelectorAll('.suggestion-item');
        suggestionItems.forEach(item => {
            item.addEventListener('click', function() {
                searchInput.value = this.querySelector('span').textContent;
                searchSuggestions.style.display = 'none';
                searchInput.focus();
            });
        });
    }
    
    // Voice Search Button
    const voiceSearchBtn = document.querySelector('.voice-search-btn');
    if (voiceSearchBtn) {
        voiceSearchBtn.addEventListener('click', function() {
            if ('webkitSpeechRecognition' in window || 'SpeechRecognition' in window) {
                const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
                const recognition = new SpeechRecognition();
                
                recognition.lang = 'tr-TR';
                recognition.continuous = false;
                recognition.interimResults = false;
                
                recognition.onstart = function() {
                    voiceSearchBtn.style.color = 'var(--danger)';
                    voiceSearchBtn.innerHTML = '<i class="fas fa-microphone-slash"></i>';
                };
                
                recognition.onresult = function(event) {
                    const transcript = event.results[0][0].transcript;
                    searchInput.value = transcript;
                };
                
                recognition.onerror = function(event) {
                    console.error('Speech recognition error:', event.error);
                    showMessage('Sesli arama hatası: ' + event.error, 'error');
                };
                
                recognition.onend = function() {
                    voiceSearchBtn.style.color = '';
                    voiceSearchBtn.innerHTML = '<i class="fas fa-microphone"></i>';
                };
                
                recognition.start();
            } else {
                showMessage('Tarayıcınız sesli aramayı desteklemiyor.', 'warning');
            }
        });
    }
    
    // Filter Chips
    const filterChips = document.querySelectorAll('.filter-chip');
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
    
    // Popular Tags
    const popularTags = document.querySelectorAll('.popular-tag');
    popularTags.forEach(tag => {
        tag.addEventListener('click', function(e) {
            // Track popular search click
            const query = this.textContent;
            console.log('Popular search clicked:', query);
        });
    });
    
    // Search Form Submit
    const searchForm = document.querySelector('.search-form');
    if (searchForm) {
        searchForm.addEventListener('submit', function(e) {
            const query = searchInput.value.trim();
            
            if (!query) {
                e.preventDefault();
                showMessage('Lütfen bir arama terimi girin.', 'warning');
                searchInput.focus();
                return;
            }
            
            // Track search
            trackSearch(query);
        });
    }
    
    // User Menu
    const userMenu = document.querySelector('.user-menu-compact');
    if (userMenu) {
        userMenu.addEventListener('click', function() {
            // Toggle dropdown menu
            console.log('User menu clicked');
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