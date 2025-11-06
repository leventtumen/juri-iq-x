using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JurisIQ.Backend.Models
{
    public enum SubscriptionType
    {
        Simple = 1, // 1 device
        Pro = 2     // 4 devices
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Simple;

        public bool IsAdmin { get; set; } = false;

        public bool IsBlocked { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        // Navigation properties
        public virtual ICollection<UserDevice> Devices { get; set; } = new List<UserDevice>();
        public virtual ICollection<SearchHistory> SearchHistory { get; set; } = new List<SearchHistory>();
        public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        public virtual ICollection<FailedLoginAttempt> FailedLoginAttempts { get; set; } = new List<FailedLoginAttempt>();
    }

    public class UserDevice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string DeviceId { get; set; } = string.Empty;

        [MaxLength(255)]
        public string DeviceName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string DeviceType { get; set; } = string.Empty;

        [MaxLength(255)]
        public string UserAgent { get; set; } = string.Empty;

        public DateTime FirstLoginAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class FailedLoginAttempt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [MaxLength(45)]
        public string IPAddress { get; set; } = string.Empty;

        [MaxLength(255)]
        public string UserAgent { get; set; } = string.Empty;

        public DateTime AttemptAt { get; set; } = DateTime.UtcNow;
    }
}