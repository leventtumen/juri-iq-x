using JuriIQ.Core.Enums;

namespace JuriIQ.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public SubscriptionType SubscriptionType { get; set; }
    public bool IsBlacklisted { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LastFailedLogin { get; set; }
    public DateTime? BlockedUntil { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    public ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();
}
