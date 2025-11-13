namespace JuriIQ.Core.Entities;

public class Device
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public DateTime LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
}
