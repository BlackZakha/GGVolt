using GGVolt.Core.Enums;

namespace GGVolt.Core.Entities;

public class License : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid GameId { get; set; }
    public Game Game { get; set; } = null!;
    
    public LicenseStatus Status { get; set; } = LicenseStatus.Active;
    public DateTime PurchasedAt { get; set; }
    public DateTime? ExpiresAt { get; set; } // null = навсегда
}