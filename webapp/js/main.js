// ============================================
// MAIN JAVASCRIPT
// ============================================

document.addEventListener('DOMContentLoaded', function() {
    // Mobile Menu Toggle
    const menuToggle = document.getElementById('menuToggle');
    const sidebar = document.getElementById('sidebar');
    
    if (menuToggle && sidebar) {
        menuToggle.addEventListener('click', function() {
            sidebar.classList.toggle('active');
            
            // Toggle hamburger animation
            this.classList.toggle('active');
        });
        
        // Close sidebar when clicking outside on mobile
        document.addEventListener('click', function(event) {
            if (window.innerWidth <= 1024) {
                if (!sidebar.contains(event.target) && !menuToggle.contains(event.target)) {
                    sidebar.classList.remove('active');
                    menuToggle.classList.remove('active');
                }
            }
        });
    }
    
    // Table Sorting
    const sortableHeaders = document.querySelectorAll('.data-table th.sortable');
    sortableHeaders.forEach(header => {
        header.addEventListener('click', function() {
            const table = this.closest('table');
            const tbody = table.querySelector('tbody');
            const rows = Array.from(tbody.querySelectorAll('tr'));
            const columnIndex = Array.from(this.parentElement.children).indexOf(this);
            const isAscending = this.classList.contains('sort-asc');
            
            // Remove sort classes from all headers
            sortableHeaders.forEach(h => {
                h.classList.remove('sort-asc', 'sort-desc');
            });
            
            // Add appropriate sort class
            if (isAscending) {
                this.classList.add('sort-desc');
            } else {
                this.classList.add('sort-asc');
            }
            
            // Sort rows
            rows.sort((a, b) => {
                const aValue = a.children[columnIndex].textContent.trim();
                const bValue = b.children[columnIndex].textContent.trim();
                
                // Try to parse as number
                const aNum = parseFloat(aValue.replace(/[^0-9.-]/g, ''));
                const bNum = parseFloat(bValue.replace(/[^0-9.-]/g, ''));
                
                if (!isNaN(aNum) && !isNaN(bNum)) {
                    return isAscending ? bNum - aNum : aNum - bNum;
                }
                
                // String comparison
                return isAscending 
                    ? bValue.localeCompare(aValue, 'tr')
                    : aValue.localeCompare(bValue, 'tr');
            });
            
            // Reorder rows
            rows.forEach(row => tbody.appendChild(row));
        });
    });
    
    // Search functionality
    const searchInputs = document.querySelectorAll('.search-box input');
    searchInputs.forEach(input => {
        input.addEventListener('input', function(e) {
            const searchTerm = e.target.value.toLowerCase();
            const table = document.querySelector('.data-table');
            
            if (table) {
                const rows = table.querySelectorAll('tbody tr');
                rows.forEach(row => {
                    const text = row.textContent.toLowerCase();
                    row.style.display = text.includes(searchTerm) ? '' : 'none';
                });
            }
        });
    });
    
    // Smooth scroll for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
    
    // Add loading state to buttons
    const buttons = document.querySelectorAll('.btn-professional');
    buttons.forEach(button => {
        button.addEventListener('click', function(e) {
            if (this.type === 'submit') {
                const originalContent = this.innerHTML;
                this.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Yükleniyor...';
                this.disabled = true;
                
                // Simulate loading (remove in production)
                setTimeout(() => {
                    this.innerHTML = originalContent;
                    this.disabled = false;
                }, 2000);
            }
        });
    });
    
    // Initialize tooltips if Bootstrap is loaded
    if (typeof bootstrap !== 'undefined') {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function(tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
    
    // Add animation on scroll
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };
    
    const observer = new IntersectionObserver(function(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, observerOptions);
    
    // Observe cards and stats
    document.querySelectorAll('.card-professional, .stats-card').forEach(el => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(20px)';
        el.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
        observer.observe(el);
    });
    
    // User menu dropdown
    const userMenu = document.querySelector('.user-menu');
    if (userMenu) {
        userMenu.addEventListener('click', function(e) {
            e.stopPropagation();
            // Add dropdown functionality here if needed
            console.log('User menu clicked');
        });
    }
    
    // Notification bell
    const notificationBtn = document.querySelector('.btn-outline .fa-bell');
    if (notificationBtn) {
        notificationBtn.parentElement.addEventListener('click', function() {
            // Add notification panel functionality here
            console.log('Notifications clicked');
        });
    }
    
    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.opacity = '0';
            setTimeout(() => alert.remove(), 300);
        }, 5000);
    });
    
    // Prevent form submission for demo
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            console.log('Form submitted (demo mode)');
            
            // Show success message
            const successMsg = document.createElement('div');
            successMsg.className = 'alert alert-success';
            successMsg.style.cssText = 'position: fixed; top: 20px; right: 20px; z-index: 9999; padding: 1rem 2rem; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);';
            successMsg.innerHTML = '<i class="fas fa-check-circle"></i> İşlem başarıyla tamamlandı!';
            document.body.appendChild(successMsg);
            
            setTimeout(() => {
                successMsg.style.opacity = '0';
                setTimeout(() => successMsg.remove(), 300);
            }, 3000);
        });
    });
});

// Utility function to format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('tr-TR', {
        style: 'currency',
        currency: 'TRY'
    }).format(amount);
}

// Utility function to format date
function formatDate(date) {
    return new Intl.DateTimeFormat('tr-TR', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    }).format(new Date(date));
}

// Export functions for use in other scripts
window.HukukPro = {
    formatCurrency,
    formatDate
};