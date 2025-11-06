namespace JurisIQ.Backend.Configuration
{
    public class AppSettings
    {
        public JwtSettings Jwt { get; set; } = new();
        public SecuritySettings Security { get; set; } = new();
        public DocumentProcessingSettings DocumentProcessing { get; set; } = new();
        public DatabaseSettings Database { get; set; } = new();
    }

    public class JwtSettings
    {
        public string SecretKey { get; set; } = "your-super-secret-key-change-this-in-production";
        public string Issuer { get; set; } = "JurisIQ";
        public string Audience { get; set; } = "JurisIQ-Users";
        public int ExpirationMinutes { get; set; } = 60;
    }

    public class SecuritySettings
    {
        public int MaxFailedLoginAttempts { get; set; } = 5;
        public int FailedLoginWindowMinutes { get; set; } = 15;
        public int AccountLockoutMinutes { get; set; } = 30;
        public int MaxDevicesSimple { get; set; } = 1;
        public int MaxDevicesPro { get; set; } = 4;
        public int DeviceViolationThreshold { get; set; } = 10;
    }

    public class DocumentProcessingSettings
    {
        public string SampleDocumentsPath { get; set; } = "../sample-documents";
        public string SupportedExtensions { get; set; } = ".pdf,.doc,.docx,.txt,.dot";
        public bool EnableDailyProcessing { get; set; } = true;
        public string DailyProcessingTime { get; set; } = "02:00";
        public int MaxDocumentSizeMB { get; set; } = 50;
    }

    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = "Data Source=jurisiq.db";
    }
}