// ============================================
// KARARLAR PAGE JAVASCRIPT
// ============================================

document.addEventListener('DOMContentLoaded', function() {
    // Get all view buttons (eye icon buttons)
    const viewButtons = document.querySelectorAll('button.btn-professional.btn-outline');
    
    viewButtons.forEach(button => {
        // Check if this button contains the eye icon
        const hasEyeIcon = button.querySelector('i.fa-eye');
        
        if (hasEyeIcon) {
            button.addEventListener('click', function(e) {
                e.preventDefault();
                
                // Get the row data
                const row = this.closest('tr');
                const cells = row.querySelectorAll('td');
                
                // Extract document information from the row
                const documentTitle = cells[0]?.textContent.trim() || 'Karar';
                const caseNumber = cells[1]?.textContent.trim() || '';
                const court = cells[2]?.textContent.trim() || '';
                const date = cells[3]?.textContent.trim() || '';
                const status = cells[4]?.textContent.trim() || '';
                
                // Create a document ID (you can modify this based on your needs)
                const documentId = `karar-${Date.now()}`;
                
                // Store document info in sessionStorage for the PDF viewer
                const documentInfo = {
                    id: documentId,
                    title: documentTitle,
                    caseNumber: caseNumber,
                    court: court,
                    date: date,
                    status: status,
                    type: 'Karar',
                    source: 'kararlar'
                };
                
                sessionStorage.setItem('currentDocument', JSON.stringify(documentInfo));
                
                // Redirect to PDF viewer
                window.location.href = `pdf-viewer.html?id=${documentId}&type=karar`;
            });
        }
    });
    
    // Add hover effect to view buttons
    viewButtons.forEach(button => {
        const hasEyeIcon = button.querySelector('i.fa-eye');
        if (hasEyeIcon) {
            button.style.cursor = 'pointer';
            button.title = 'Görüntüle';
        }
    });
});