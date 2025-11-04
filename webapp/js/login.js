// ============================================
// LOGIN PAGE JAVASCRIPT
// ============================================

document.addEventListener('DOMContentLoaded', function() {
    // Password Toggle
    const togglePassword = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('password');
    
    if (togglePassword && passwordInput) {
        togglePassword.addEventListener('click', function() {
            const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
            passwordInput.setAttribute('type', type);
            
            // Toggle icon
            const icon = this.querySelector('i');
            icon.classList.toggle('fa-eye');
            icon.classList.toggle('fa-eye-slash');
        });
    }
    
    // Login Form Submission
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const email = document.getElementById('email').value;
            const password = document.getElementById('password').value;
            const remember = document.getElementById('remember').checked;
            
            // Validate inputs
            if (!email || !password) {
                showMessage('Lütfen tüm alanları doldurun.', 'error');
                return;
            }
            
            // Show loading state
            const submitBtn = this.querySelector('button[type="submit"]');
            const originalContent = submitBtn.innerHTML;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Giriş yapılıyor...';
            submitBtn.disabled = true;
            
            try {
                // Generate or retrieve device ID
                let deviceId = localStorage.getItem('deviceId');
                if (!deviceId) {
                    deviceId = 'web-' + Date.now() + '-' + Math.random().toString(36).substr(2, 9);
                    localStorage.setItem('deviceId', deviceId);
                }
                
                // Use API service for login
                const data = await apiService.login(
                    email,
                    password,
                    deviceId,
                    navigator.userAgent || 'Unknown Browser',
                    'Web'
                );
                
                // Store user info if needed
                if (data.user) {
                    localStorage.setItem('userInfo', JSON.stringify(data.user));
                }
                
                // Success message
                showMessage('Giriş başarılı! Yönlendiriliyorsunuz...', 'success');
                
                // Redirect to search page
                setTimeout(() => {
                    window.location.href = 'ana-sayfa.html';
                }, 1500);
                
            } catch (error) {
                console.error('Login error:', error);
                showMessage('Giriş başarısız oldu. Lütfen bilgilerinizi kontrol edin.', 'error');
                // Reset button
                submitBtn.innerHTML = originalContent;
                submitBtn.disabled = false;
            }
        });
    }
    
    // Social login buttons
    const socialBtns = document.querySelectorAll('.social-btn');
    socialBtns.forEach(btn => {
        btn.addEventListener('click', function() {
            const provider = this.querySelector('i').classList.contains('fa-google') ? 'Google' : 'Diğer';
            showMessage(`${provider} ile giriş yapılıyor...`, 'info');
        });
    });
    
    // Forgot password link
    const forgotPasswordLink = document.querySelector('.forgot-password');
    if (forgotPasswordLink) {
        forgotPasswordLink.addEventListener('click', function(e) {
            e.preventDefault();
            showMessage('Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.', 'info');
        });
    }
    
    // Input validation on blur
    const inputs = document.querySelectorAll('.form-control-professional');
    inputs.forEach(input => {
        input.addEventListener('blur', function() {
            validateInput(this);
        });
        
        input.addEventListener('input', function() {
            // Remove error state on input
            this.classList.remove('is-invalid');
            const errorMsg = this.parentElement.querySelector('.error-message');
            if (errorMsg) {
                errorMsg.remove();
            }
        });
    });
    
    // Add enter key support
    document.addEventListener('keypress', function(e) {
        if (e.key === 'Enter' && loginForm) {
            loginForm.dispatchEvent(new Event('submit'));
        }
    });
});

// Validate individual input
function validateInput(input) {
    const value = input.value.trim();
    const type = input.type;
    let isValid = true;
    let errorMessage = '';
    
    if (!value) {
        isValid = false;
        errorMessage = 'Bu alan zorunludur.';
    } else if (type === 'email' && !isValidEmail(value)) {
        isValid = false;
        errorMessage = 'Geçerli bir e-posta adresi girin.';
    } else if (type === 'password' && value.length < 6) {
        isValid = false;
        errorMessage = 'Şifre en az 6 karakter olmalıdır.';
    }
    
    if (!isValid) {
        input.classList.add('is-invalid');
        showInputError(input, errorMessage);
    } else {
        input.classList.remove('is-invalid');
        input.classList.add('is-valid');
        removeInputError(input);
    }
    
    return isValid;
}

// Email validation
function isValidEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
}

// Show input error
function showInputError(input, message) {
    removeInputError(input);
    
    const errorDiv = document.createElement('div');
    errorDiv.className = 'error-message';
    errorDiv.style.cssText = 'color: var(--danger); font-size: var(--fs-xs); margin-top: var(--spacing-xs);';
    errorDiv.textContent = message;
    
    input.parentElement.appendChild(errorDiv);
}

// Remove input error
function removeInputError(input) {
    const errorMsg = input.parentElement.querySelector('.error-message');
    if (errorMsg) {
        errorMsg.remove();
    }
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
    
    .form-control-professional.is-invalid {
        border-color: var(--danger);
    }
    
    .form-control-professional.is-valid {
        border-color: var(--success);
    }
`;
document.head.appendChild(style);