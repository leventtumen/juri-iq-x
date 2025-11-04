// ============================================
// PDF VIEWER PAGE JAVASCRIPT
// ============================================

document.addEventListener('DOMContentLoaded', function() {
    // Get document info from sessionStorage
    const documentInfoStr = sessionStorage.getItem('currentDocument');
    
    if (documentInfoStr) {
        try {
            const documentInfo = JSON.parse(documentInfoStr);
            
            // Update the page title
            const titleElement = document.querySelector('.pdf-title');
            if (titleElement && documentInfo.title) {
                let titleText = documentInfo.title;
                
                // Add additional info based on document type
                if (documentInfo.source === 'kararlar' && documentInfo.court) {
                    titleText = `${documentInfo.court} - ${documentInfo.title}`;
                } else if (documentInfo.source === 'mevzuatlar' && documentInfo.number) {
                    titleText = `${documentInfo.title} - ${documentInfo.number}`;
                }
                
                titleElement.textContent = titleText;
            }
            
            // Update browser title
            if (documentInfo.title) {
                document.title = `${documentInfo.title} - JuriIQ`;
            }
            
            // You can also display additional document metadata in the sidebar
            displayDocumentMetadata(documentInfo);
            
        } catch (error) {
            console.error('Error loading document info:', error);
        }
    } else {
        // If no document info, show a default message
        console.log('No document information found');
    }
    
    // Get URL parameters
    const urlParams = new URLSearchParams(window.location.search);
    const documentId = urlParams.get('id');
    const documentType = urlParams.get('type');
    
    console.log('Document ID:', documentId);
    console.log('Document Type:', documentType);
});

// Function to display document metadata
function displayDocumentMetadata(documentInfo) {
    // Find the left panel or create a metadata section
    const leftPanel = document.getElementById('leftPanel');
    
    if (leftPanel) {
        // Create metadata section
        const metadataSection = document.createElement('div');
        metadataSection.className = 'document-metadata';
        metadataSection.style.cssText = `
            padding: 20px;
            background: white;
            border-radius: 8px;
            margin: 20px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        `;
        
        let metadataHTML = '<h4 style="margin-bottom: 15px; color: #1a2332;">Belge Bilgileri</h4>';
        
        // Add metadata based on document source
        if (documentInfo.source === 'kararlar') {
            metadataHTML += `
                <div style="margin-bottom: 10px;">
                    <strong>Başlık:</strong> ${documentInfo.title || 'N/A'}
                </div>
                <div style="margin-bottom: 10px;">
                    <strong>Dava No:</strong> ${documentInfo.caseNumber || 'N/A'}
                </div>
                <div style="margin-bottom: 10px;">
                    <strong>Mahkeme:</strong> ${documentInfo.court || 'N/A'}
                </div>
                <div style="margin-bottom: 10px;">
                    <strong>Tarih:</strong> ${documentInfo.date || 'N/A'}
                </div>
                <div style="margin-bottom: 10px;">
                    <strong>Durum:</strong> ${documentInfo.status || 'N/A'}
                </div>
            `;
        } else if (documentInfo.source === 'mevzuatlar') {
            metadataHTML += `
                <div style="margin-bottom: 10px;">
                    <strong>Başlık:</strong> ${documentInfo.title || 'N/A'}
                </div>
                <div style="margin-bottom: 10px;">
                    <strong>Numara:</strong> ${documentInfo.number || 'N/A'}
                </div>
                <div style="margin-bottom: 10px;">
                    <strong>Tarih:</strong> ${documentInfo.date || 'N/A'}
                </div>
                <div style="margin-bottom: 10px;">
                    <strong>Tür:</strong> ${documentInfo.type || 'N/A'}
                </div>
            `;
        }
        
        metadataSection.innerHTML = metadataHTML;
        
        // Insert at the beginning of left panel
        if (leftPanel.firstChild) {
            leftPanel.insertBefore(metadataSection, leftPanel.firstChild);
        } else {
            leftPanel.appendChild(metadataSection);
        }
    }
}

// Add back button functionality
document.addEventListener('DOMContentLoaded', function() {
    // Find all back buttons or links
    const backButtons = document.querySelectorAll('a[href*="case-detail"], a[href*="search"]');
    
    backButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            // Check if we came from mevzuatlar or kararlar
            const documentInfoStr = sessionStorage.getItem('currentDocument');
            if (documentInfoStr) {
                try {
                    const documentInfo = JSON.parse(documentInfoStr);
                    
                    // If user wants to go back to the source page
                    if (this.getAttribute('href') === 'case-detail.html') {
                        e.preventDefault();
                        
                        if (documentInfo.source === 'mevzuatlar') {
                            window.location.href = 'mevzuatlar.html';
                        } else if (documentInfo.source === 'kararlar') {
                            window.location.href = 'kararlar.html';
                        } else {
                            window.location.href = 'ana-sayfa.html';
                        }
                    }
                } catch (error) {
                    console.error('Error processing back navigation:', error);
                }
            }
        });
    });
});