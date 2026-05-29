using GGVolt.Core.Enums;

namespace GGVolt.Core.Entities;

public class Transaction : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "RUB";
    public string ProviderTransactionId { get; set; } = string.Empty;
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public string? PaymentGatewayResponse { get; set; } // JSON для отладки/аудита
}