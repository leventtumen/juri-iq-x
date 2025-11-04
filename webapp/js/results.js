// ============================================
// SEARCH RESULTS PAGE JAVASCRIPT
// ============================================

// Global variables
let currentPage = 1;
let currentQuery = '';
let currentFilters = {};
let totalPages = 1;

document.addEventListener('DOMContentLoaded', function() {
    // Check authentication
    if (!apiService.isAuthenticated()) {
        window.location.href = 'login.html';
        return;
    }

    // Get search query from URL
    const urlParams = new URLSearchParams(window.location.search);
    currentQuery = urlParams.get('q') || '';
    
    // Update search input with current query
    const searchInput = document.querySelector('.search-input-compact, input[name="q"]');
    if (searchInput && currentQuery) {
        searchInput.value = currentQuery;
    }

    // Load search results
    if (currentQuery) {
        loadSearchResults();
    }

    // Toggle Filters Sidebar (Mobile)
    const toggleFiltersBtn = document.getElementById('toggleFilters');
    const filtersSidebar = document.querySelector('.filters-sidebar');
    
    if (toggleFiltersBtn && filtersSidebar) {
        toggleFiltersBtn.addEventListener('click', function() {
            filtersSidebar.classList.toggle('active');
        });
        
        // Close filters when clicking outside
        filtersSidebar.addEventListener('click', function(e) {
            if (e.target === this) {
                this.classList.remove('active');
            }
        });
    }
    
    // Clear Filters
    const clearFiltersBtn = document.getElementById('clearFilters');
    if (clearFiltersBtn) {
        clearFiltersBtn.addEventListener('click', function() {
            // Uncheck all checkboxes
            document.querySelectorAll('.filter-checkbox input[type="checkbox"]').forEach(cb => {
                cb.checked = false;
            });
            
            // Reset radio buttons to "all"
            document.querySelectorAll('.filter-radio input[type="radio"]').forEach(radio => {
                if (radio.value === 'all') {
                    radio.checked = true;
                }
            });
            
            // Clear filters and reload
            currentFilters = {};
            currentPage = 1;
            loadSearchResults();
            
            showMessage('Filtreler temizlendi', 'success');
        });
    }
    
    // Date Range Custom
    const customDateRadio = document.querySelector('input[type="radio"][value="custom"]');
    const dateInputs = document.querySelector('.date-inputs');
    
    if (customDateRadio && dateInputs) {
        document.querySelectorAll('input[type="radio"][name="date"]').forEach(radio => {
            radio.addEventListener('change', function() {
                if (this.value === 'custom') {
                    dateInputs.style.display = 'flex';
                } else {
                    dateInputs.style.display = 'none';
                }
            });
        });
    }
    
    // Sort Select
    const sortSelect = document.querySelector('.sort-select');
    if (sortSelect) {
        sortSelect.addEventListener('change', function() {
            const sortValue = this.value;
            console.log('Sorting by:', sortValue);
            currentFilters.sortBy = sortValue;
            currentPage = 1;
            loadSearchResults();
        });
    }
    
    // Filter Checkboxes - Track changes
    const filterCheckboxes = document.querySelectorAll('.filter-checkbox input, .filter-radio input');
    filterCheckboxes.forEach(input => {
        input.addEventListener('change', function() {
            applyFilters();
        });
    });
    
    // Apply Filters button (if exists)
    const applyFiltersBtn = document.getElementById('applyFilters');
    if (applyFiltersBtn) {
        applyFiltersBtn.addEventListener('click', function() {
            applyFilters();
        });
    }
});

// Load search results from API
async function loadSearchResults() {
    try {
        showLoadingState();
        
        // Prepare filters
        const filters = {
            page: currentPage,
            pageSize: 20,
            ...currentFilters
        };
        
        // Make API call
        const data = await apiService.searchDocuments(currentQuery, filters);
        
        // Update UI with results
        displayResults(data);
        updatePagination(data);
        
    } catch (error) {
        console.error('Error loading search results:', error);
        showMessage('Arama sonuçları yüklenirken bir hata oluştu.', 'error');
        displayNoResults();
    }
}

// Display search results
function displayResults(data) {
    const resultsContainer = document.querySelector('.results-list');
    if (!resultsContainer) return;
    
    // Update results count
    const resultsInfo = document.querySelector('.results-info');
    if (resultsInfo) {
        resultsInfo.textContent = `${data.totalCount} sonuç bulundu`;
    }
    
    // Clear existing results
    resultsContainer.innerHTML = '';
    
    if (!data.documents || data.documents.length === 0) {
        displayNoResults();
        return;
    }
    
    // Display each result
    data.documents.forEach(doc => {
        const resultItem = createResultItem(doc);
        resultsContainer.appendChild(resultItem);
    });
    
    // Highlight search terms
    highlightSearchTerms();
}

// Create result item HTML
function createResultItem(doc) {
    const div = document.createElement('div');
    div.className = 'result-item';
    
    const formattedDate = doc.decisionDate ? new Date(doc.decisionDate).toLocaleDateString('tr-TR') : 'Tarih belirtilmemiş';
    
    div.innerHTML = `
        <div class="result-header">
            <h3 class="result-title">
                <a href="case-detail.html?id=${doc.id}">${escapeHtml(doc.title)}</a>
            </h3>
            <div class="result-actions">
                <button class="action-btn bookmark-btn" data-id="${doc.id}" title="Kaydet">
                    <i class="far fa-bookmark"></i>
                </button>
                <button class="action-btn share-btn" data-id="${doc.id}" title="Paylaş">
                    <i class="fas fa-share-alt"></i>
                </button>
                <button class="action-btn download-btn" data-id="${doc.id}" title="İndir">
                    <i class="fas fa-file-pdf"></i>
                </button>
            </div>
        </div>
        
        <div class="result-meta">
            <span class="meta-item">
                <i class="fas fa-gavel"></i>
                ${escapeHtml(doc.courtName)}
            </span>
            <span class="meta-item">
                <i class="fas fa-calendar"></i>
                ${formattedDate}
            </span>
            <span class="meta-item">
                <i class="fas fa-file-alt"></i>
                ${escapeHtml(doc.documentType)}
            </span>
            <span class="meta-item">
                <i class="fas fa-hashtag"></i>
                ${escapeHtml(doc.caseNumber)}
            </span>
        </div>
        
        <p class="result-summary">${escapeHtml(doc.excerpt)}</p>
        
        <div class="result-footer">
            <span class="word-count">${doc.wordCount} kelime</span>
        </div>
    `;
    
    // Add event listeners
    const bookmarkBtn = div.querySelector('.bookmark-btn');
    bookmarkBtn.addEventListener('click', () => toggleBookmark(doc.id, bookmarkBtn));
    
    const shareBtn = div.querySelector('.share-btn');
    shareBtn.addEventListener('click', () => shareResult(doc));
    
    const downloadBtn = div.querySelector('.download-btn');
    downloadBtn.addEventListener('click', () => downloadDocument(doc.id));
    
    return div;
}

// Toggle bookmark
async function toggleBookmark(docId, button) {
    try {
        const icon = button.querySelector('i');
        
        if (icon.classList.contains('far')) {
            // Add bookmark
            await apiService.bookmarkDocument(docId);
            icon.classList.remove('far');
            icon.classList.add('fas');
            showMessage('Kaydedildi', 'success');
        } else {
            // Remove bookmark
            await apiService.removeBookmark(docId);
            icon.classList.remove('fas');
            icon.classList.add('far');
            showMessage('Kayıtlardan kaldırıldı', 'info');
        }
    } catch (error) {
        console.error('Bookmark error:', error);
        showMessage('İşlem başarısız oldu', 'error');
    }
}

// Share result
function shareResult(doc) {
    const url = `${window.location.origin}/case-detail.html?id=${doc.id}`;
    
    if (navigator.share) {
        navigator.share({
            title: doc.title,
            url: url
        }).then(() => {
            showMessage('Paylaşıldı', 'success');
        }).catch(err => {
            console.error('Share error:', err);
            copyToClipboard(url);
        });
    } else {
        copyToClipboard(url);
    }
}

// Download document
function downloadDocument(docId) {
    showMessage('PDF indiriliyor...', 'info');
    // In a real implementation, this would trigger a download
    console.log('Downloading document:', docId);
}

// Update pagination
function updatePagination(data) {
    totalPages = data.totalPages || 1;
    
    const paginationContainer = document.querySelector('.pagination-controls');
    if (!paginationContainer) return;
    
    paginationContainer.innerHTML = '';
    
    // Previous button
    const prevBtn = document.createElement('button');
    prevBtn.className = 'page-btn';
    prevBtn.innerHTML = '<i class="fas fa-chevron-left"></i>';
    prevBtn.disabled = currentPage === 1;
    prevBtn.addEventListener('click', () => goToPage(currentPage - 1));
    paginationContainer.appendChild(prevBtn);
    
    // Page numbers
    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(totalPages, currentPage + 2);
    
    for (let i = startPage; i <= endPage; i++) {
        const pageBtn = document.createElement('button');
        pageBtn.className = 'page-btn';
        if (i === currentPage) pageBtn.classList.add('active');
        pageBtn.textContent = i;
        pageBtn.addEventListener('click', () => goToPage(i));
        paginationContainer.appendChild(pageBtn);
    }
    
    // Next button
    const nextBtn = document.createElement('button');
    nextBtn.className = 'page-btn';
    nextBtn.innerHTML = '<i class="fas fa-chevron-right"></i>';
    nextBtn.disabled = currentPage === totalPages;
    nextBtn.addEventListener('click', () => goToPage(currentPage + 1));
    paginationContainer.appendChild(nextBtn);
}

// Go to page
function goToPage(page) {
    if (page < 1 || page > totalPages) return;
    currentPage = page;
    loadSearchResults();
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// Apply filters
function applyFilters() {
    currentFilters = {};
    
    // Get document type filters
    const typeCheckboxes = document.querySelectorAll('.filter-checkbox input[type="checkbox"]:checked');
    const types = Array.from(typeCheckboxes).map(cb => cb.value).filter(v => v);
    if (types.length > 0) {
        currentFilters.documentType = types.join(',');
    }
    
    // Get date filter
    const dateRadio = document.querySelector('input[name="date"]:checked');
    if (dateRadio && dateRadio.value !== 'all') {
        if (dateRadio.value === 'custom') {
            const dateFrom = document.querySelector('input[name="dateFrom"]')?.value;
            const dateTo = document.querySelector('input[name="dateTo"]')?.value;
            if (dateFrom) currentFilters.dateFrom = dateFrom;
            if (dateTo) currentFilters.dateTo = dateTo;
        } else {
            // Handle predefined date ranges
            const days = parseInt(dateRadio.value);
            const dateFrom = new Date();
            dateFrom.setDate(dateFrom.getDate() - days);
            currentFilters.dateFrom = dateFrom.toISOString().split('T')[0];
        }
    }
    
    // Reset to first page and reload
    currentPage = 1;
    loadSearchResults();
}

// Show loading state
function showLoadingState() {
    const resultsContainer = document.querySelector('.results-list');
    if (!resultsContainer) return;
    
    resultsContainer.innerHTML = `
        <div class="loading-state" style="text-align: center; padding: 3rem;">
            <i class="fas fa-spinner fa-spin" style="font-size: 2rem; color: var(--primary);"></i>
            <p style="margin-top: 1rem; color: var(--text-secondary);">Sonuçlar yükleniyor...</p>
        </div>
    `;
}

// Display no results
function displayNoResults() {
    const resultsContainer = document.querySelector('.results-list');
    if (!resultsContainer) return;
    
    resultsContainer.innerHTML = `
        <div class="no-results" style="text-align: center; padding: 3rem;">
            <i class="fas fa-search" style="font-size: 3rem; color: var(--text-tertiary);"></i>
            <h3 style="margin-top: 1rem;">Sonuç bulunamadı</h3>
            <p style="color: var(--text-secondary);">Arama kriterlerinizi değiştirerek tekrar deneyin.</p>
        </div>
    `;
}

// Highlight search terms
function highlightSearchTerms() {
    if (!currentQuery) return;
    
    const terms = currentQuery.toLowerCase().split(' ');
    const summaries = document.querySelectorAll('.result-summary');
    
    summaries.forEach(summary => {
        let html = summary.innerHTML;
        terms.forEach(term => {
            if (term.length > 2) {
                const regex = new RegExp(`(${term})`, 'gi');
                html = html.replace(regex, '<mark class="highlight">$1</mark>');
            }
        });
        summary.innerHTML = html;
    });
}

// Copy to clipboard
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(() => {
        showMessage('Link kopyalandı', 'success');
    }).catch(err => {
        console.error('Copy error:', err);
        showMessage('Kopyalama başarısız', 'error');
    });
}

// Escape HTML to prevent XSS
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
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

// Add CSS for highlights
const style = document.createElement('style');
style.textContent = `
    .highlight {
        background-color: #fff3cd;
        padding: 0.1em 0.2em;
        border-radius: 2px;
        font-weight: 500;
    }
    
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