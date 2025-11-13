-- ============================================
-- JuriIQ Database Seed Data
-- Insert initial test data
-- ============================================

-- Insert admin user
-- Email: admin@test.com
-- Password: Pass!2345 (hashed with BCrypt)
INSERT INTO users (email, password_hash, first_name, last_name, is_admin, subscription_type, created_at)
VALUES (
    'admin@test.com',
    '$2a$11$YourHashedPasswordHere', -- This will be replaced by actual BCrypt hash
    'Admin',
    'User',
    TRUE,
    2, -- Pro subscription
    CURRENT_TIMESTAMP
) ON CONFLICT (email) DO NOTHING;

-- Note: The password hash will be updated in the application initialization
-- BCrypt hash for "Pass!2345" will be generated at runtime
