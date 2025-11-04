// ============================================
// CASE DETAIL PAGE JAVASCRIPT
// ============================================

let currentDocumentId = null;
let currentDocument = null;

document.addEventListener('DOMContentLoaded', function() {
    // Temporarily comment out authentication check for testing
    // TODO: Restore authentication check after testing
    // if (!apiService.isAuthenticated()) {
    //     window.location.href = 'login.html';
    //     return;
    // }

    // Get document ID from URL
    const urlParams = new URLSearchParams(window.location.search);
    currentDocumentId = urlParams.get('id');
    
    if (!currentDocumentId) {
        showMessage('Geçersiz belge ID', 'error');
        setTimeout(() => {
            window.location.href = 'ana-sayfa.html';
        }, 2000);
        return;
    }
    
    // Load document details
    loadDocumentDetails();
    
    // Load statistics
    loadDocumentStatistics();
    
    // Load AI summary
    loadAiSummary();
    
    // Load related documents
    loadRelatedDocuments();
    
    // Setup event listeners
    setupEventListeners();
});

// Load document details from API
async function loadDocumentDetails() {
    try {
        showLoadingState();
        
        // Fetch document from API
        currentDocument = await apiService.getDocument(currentDocumentId);
        
        // Display document details
        displayDocumentDetails(currentDocument);
        
    } catch (error) {
        console.error('Error loading document:', error);
        showMessage('Belge yüklenirken bir hata oluştu.', 'error');
        displayErrorState();
    }
}

// Load document statistics
async function loadDocumentStatistics() {
    try {
        const stats = await apiService.getDocumentStatistics(currentDocumentId);
        
        // Update view count
        const viewCountElement = document.querySelector('.info-item .info-value');
        if (viewCountElement &amp;&amp; viewCountElement.textContent.includes('kez')) {
            viewCountElement.textContent = `${stats.viewCount} kez`;
        }
        
        // Update bookmark count
        const bookmarkElements = document.querySelectorAll('.info-item .info-value');
        if (bookmarkElements.length > 1) {
            bookmarkElements[1].textContent = `${stats.bookmarkCount} kullanıcı`;
        }
        
    } catch (error) {
        console.error('Error loading statistics:', error);
        // Don't show error message for statistics - it's not critical
    }
}

// Load AI summary
async function loadAiSummary() {
    const aiSummaryCard = document.querySelector('.ai-summary');
    if (!aiSummaryCard) return;
    
    try {
        // Check if document already has AI summary
        if (currentDocument &amp;&amp; currentDocument.aiSummary) {
            displayAiSummary(currentDocument.aiSummary, currentDocument.aiKeyPoints);
            return;
        }
        
        // Show loading state
        const aiContent = aiSummaryCard.querySelector('.ai-content');
        if (aiContent) {
            aiContent.innerHTML = '<div class="loading-spinner">Yapay zeka özeti oluşturuluyor...</div>';
        }
        
        // Generate AI summary
        const result = await apiService.generateDocumentSummary(currentDocumentId);
        
        // Display the summary
        displayAiSummary(result.summary, result.keyPoints);
        
    } catch (error) {
        console.error('Error loading AI summary:', error);
        const aiContent = aiSummaryCard.querySelector('.ai-content');
        if (aiContent) {
            aiContent.innerHTML = `
                <p style="color: var(--text-secondary);">
                    <i class="fas fa-exclamation-circle"></i>
                    Yapay zeka özeti şu anda kullanılamıyor.
                </p>
                <button class="btn btn-sm btn-primary" onclick="retryAiSummary()">
                    <i class="fas fa-redo"></i> Tekrar Dene
                </button>
            `;
        }
    }
}

// Display AI summary
function displayAiSummary(summary, keyPoints) {
    const aiContent = document.querySelector('.ai-summary .ai-content');
    if (!aiContent) return;
    
    let keyPointsList = [];
    if (typeof keyPoints === 'string') {
        try {
            keyPointsList = JSON.parse(keyPoints);
        } catch (e) {
            keyPointsList = [];
        }
    } else if (Array.isArray(keyPoints)) {
        keyPointsList = keyPoints;
    }
    
    let html = `<p><strong>Basit Dille:</strong></p><p>${escapeHtml(summary)}</p>`;
    
    if (keyPointsList.length > 0) {
        html += '<p><strong>Sizin İçin Önemli:</strong></p><ul>';
        keyPointsList.forEach(point => {
            html += `<li>${escapeHtml(point)}</li>`;
        });
        html += '</ul>';
    }
    
    aiContent.innerHTML = html;
}

// Retry AI summary generation
window.retryAiSummary = function() {
    loadAiSummary();
};

// Load related documents
async function loadRelatedDocuments() {
    try {
        const relatedDocs = await apiService.getRelatedDocuments(currentDocumentId, 5);
        
        // Find or create related documents section
        let relatedSection = document.querySelector('.related-documents');
        if (!relatedSection) {
            // Create related documents section if it doesn't exist
            const sidebar = document.querySelector('.sidebar-content');
            if (sidebar) {
                relatedSection = document.createElement('div');
                relatedSection.className = 'sidebar-card related-documents';
                relatedSection.innerHTML = `
                    <h3 class="sidebar-title">
                        <i class="fas fa-link"></i>
                        İlgili Kararlar
                    </h3>
                    <div class="related-list"></div>
                `;
                sidebar.appendChild(relatedSection);
            }
        }
        
        const relatedList = relatedSection?.querySelector('.related-list');
        if (relatedList &amp;&amp; relatedDocs.length > 0) {
            relatedList.innerHTML = relatedDocs.map(doc => `
                <a href="case-detail.html?id=${doc.id}" class="related-item">
                    <div class="related-title">${escapeHtml(doc.title)}</div>
                    <div class="related-meta">
                        <span>${escapeHtml(doc.caseNumber)}</span>
                        <span>${doc.decisionDate ? new Date(doc.decisionDate).toLocaleDateString('tr-TR') : ''}</span>
                    </div>
                </a>
            `).join('');
        } else if (relatedList) {
            relatedList.innerHTML = '<p style="color: var(--text-secondary); font-size: var(--fs-sm);">İlgili karar bulunamadı.</p>';
        }
        
    } catch (error) {
        console.error('Error loading related documents:', error);
        // Don't show error message - it's not critical
    }
}

// Display document details
function displayDocumentDetails(doc) {
    // Update page title
    document.title = `${doc.title} - JuriIQ`;
    
    // Update document title
    const titleElement = document.querySelector('.document-title, .case-title');
    if (titleElement) {
        titleElement.textContent = doc.title;
    }
    
    // Update case number
    const caseNumberElement = document.querySelector('.case-number');
    if (caseNumberElement) {
        caseNumberElement.textContent = doc.caseNumber;
    }
    
    // Update court name
    const courtElement = document.querySelector('.court-name');
    if (courtElement) {
        courtElement.textContent = doc.courtName;
    }
    
    // Update decision date
    const dateElement = document.querySelector('.decision-date');
    if (dateElement && doc.decisionDate) {
        const formattedDate = new Date(doc.decisionDate).toLocaleDateString('tr-TR', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
        dateElement.textContent = formattedDate;
    }
    
    // Update document type
    const typeElement = document.querySelector('.document-type');
    if (typeElement) {
        typeElement.textContent = doc.documentType;
    }
    
    // Update word count
    const wordCountElement = document.querySelector('.word-count');
    if (wordCountElement) {
        wordCountElement.textContent = `${doc.wordCount} kelime`;
    }
    
    // Update document content
    const contentElement = document.querySelector('.document-content, .case-content');
    if (contentElement) {
        // Format the extracted text with paragraphs
        const formattedText = formatDocumentText(doc.extractedText);
        contentElement.innerHTML = formattedText;
    }
    
    // Update keywords
    const keywordsContainer = document.querySelector('.keywords-container, .tags-container');
    if (keywordsContainer && doc.keywords) {
        keywordsContainer.innerHTML = '';
        doc.keywords.forEach(keyword => {
            const tag = document.createElement('span');
            tag.className = 'keyword-tag';
            tag.textContent = keyword;
            keywordsContainer.appendChild(tag);
        });
    }
    
    // Update metadata section
    updateMetadataSection(doc);
}

// Format document text into paragraphs
function formatDocumentText(text) {
    if (!text) return '<p>İçerik bulunamadı.</p>';
    
    // Split by double newlines or periods followed by newlines
    const paragraphs = text.split(/\n\n+|\.\s*\n/);
    
    return paragraphs
        .filter(p => p.trim().length > 0)
        .map(p => `<p>${escapeHtml(p.trim())}</p>`)
        .join('');
}

// Update metadata section
function updateMetadataSection(doc) {
    const metadataContainer = document.querySelector('.metadata-section, .case-metadata');
    if (!metadataContainer) return;
    
    const metadata = [
        { icon: 'fa-file-alt', label: 'Dosya Adı', value: doc.fileName },
        { icon: 'fa-file-code', label: 'Dosya Türü', value: doc.fileType },
        { icon: 'fa-hashtag', label: 'Dava No', value: doc.caseNumber },
        { icon: 'fa-gavel', label: 'Mahkeme', value: doc.courtName },
        { icon: 'fa-calendar', label: 'Karar Tarihi', value: doc.decisionDate ? new Date(doc.decisionDate).toLocaleDateString('tr-TR') : 'Belirtilmemiş' },
        { icon: 'fa-tag', label: 'Belge Türü', value: doc.documentType },
        { icon: 'fa-align-left', label: 'Kelime Sayısı', value: doc.wordCount }
    ];
    
    metadataContainer.innerHTML = metadata.map(item => `
        <div class="metadata-item">
            <i class="fas ${item.icon}"></i>
            <div class="metadata-content">
                <span class="metadata-label">${item.label}</span>
                <span class="metadata-value">${escapeHtml(String(item.value))}</span>
            </div>
        </div>
    `).join('');
}

// Setup event listeners
function setupEventListeners() {
    // Bookmark button
    const bookmarkBtn = document.querySelector('.bookmark-btn, .btn-bookmark');
    if (bookmarkBtn) {
        bookmarkBtn.addEventListener('click', toggleBookmark);
    }
    
    // Share button
    const shareBtn = document.querySelector('.share-btn, .btn-share');
    if (shareBtn) {
        shareBtn.addEventListener('click', shareDocument);
    }
    
    // Download button
    const downloadBtn = document.querySelector('.download-btn, .btn-download');
    if (downloadBtn) {
        downloadBtn.addEventListener('click', downloadDocument);
    }
    
    // Print button
    const printBtn = document.querySelector('.print-btn, .btn-print');
    if (printBtn) {
        printBtn.addEventListener('click', printDocument);
    }
    
    // Back button
    const backBtn = document.querySelector('.back-btn, .btn-back');
    if (backBtn) {
        backBtn.addEventListener('click', () => {
            window.history.back();
        });
    }
}

// Toggle bookmark
async function toggleBookmark() {
    if (!currentDocumentId) return;
    
    try {
        const bookmarkBtn = document.querySelector('.bookmark-btn, .btn-bookmark');
        const icon = bookmarkBtn?.querySelector('i');
        
        if (icon && icon.classList.contains('far')) {
            // Add bookmark
            await apiService.bookmarkDocument(currentDocumentId);
            icon.classList.remove('far');
            icon.classList.add('fas');
            showMessage('Belge kaydedildi', 'success');
        } else if (icon) {
            // Remove bookmark
            await apiService.removeBookmark(currentDocumentId);
            icon.classList.remove('fas');
            icon.classList.add('far');
            showMessage('Belge kayıtlardan kaldırıldı', 'info');
        }
    } catch (error) {
        console.error('Bookmark error:', error);
        showMessage('İşlem başarısız oldu', 'error');
    }
}

// Share document
function shareDocument() {
    if (!currentDocument) return;
    
    const url = window.location.href;
    
    if (navigator.share) {
        navigator.share({
            title: currentDocument.title,
            text: `${currentDocument.caseNumber} - ${currentDocument.courtName}`,
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
function downloadDocument() {
    if (!currentDocument) return;
    
    showMessage('Belge indiriliyor...', 'info');
    // In a real implementation, this would trigger a download
    console.log('Downloading document:', currentDocumentId);
}

// Print document
function printDocument() {
    window.print();
}

// Show loading state
function showLoadingState() {
    const mainContent = document.querySelector('.case-detail-container, .document-container, main');
    if (!mainContent) return;
    
    mainContent.innerHTML = `
        <div class="loading-state" style="text-align: center; padding: 3rem;">
            <i class="fas fa-spinner fa-spin" style="font-size: 3rem; color: var(--primary);"></i>
            <p style="margin-top: 1rem; color: var(--text-secondary);">Belge yükleniyor...</p>
        </div>
    `;
}

// Display error state
function displayErrorState() {
    const mainContent = document.querySelector('.case-detail-container, .document-container, main');
    if (!mainContent) return;
    
    mainContent.innerHTML = `
        <div class="error-state" style="text-align: center; padding: 3rem;">
            <i class="fas fa-exclamation-circle" style="font-size: 3rem; color: var(--danger);"></i>
            <h3 style="margin-top: 1rem;">Belge yüklenemedi</h3>
            <p style="color: var(--text-secondary);">Belge bulunamadı veya erişim izniniz yok.</p>
            <button onclick="window.history.back()" class="btn btn-primary" style="margin-top: 1rem;">
                Geri Dön
            </button>
        </div>
    `;
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
    
    .keyword-tag {
        display: inline-block;
        padding: 0.25rem 0.75rem;
        margin: 0.25rem;
        background: var(--primary-light);
        color: var(--primary);
        border-radius: var(--border-radius-sm);
        font-size: var(--fs-sm);
    }
    
    .metadata-item {
        display: flex;
        align-items: flex-start;
        gap: 1rem;
        padding: 0.75rem 0;
        border-bottom: 1px solid var(--border-color);
    }
    
    .metadata-item:last-child {
        border-bottom: none;
    }
    
    .metadata-item i {
        color: var(--primary);
        font-size: 1.2rem;
        margin-top: 0.25rem;
    }
    
    .metadata-content {
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
    }
    
    .metadata-label {
        font-size: var(--fs-sm);
        color: var(--text-secondary);
        font-weight: 500;
    }
    
    .metadata-value {
        color: var(--text-primary);
    }
`;
document.head.appendChild(style);