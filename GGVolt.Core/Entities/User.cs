namespace GGVolt.Core.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    // Навигация
    public ICollection<License> Licenses { get; set; } = new List<License>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}