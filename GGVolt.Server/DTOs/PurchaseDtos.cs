namespace GGVolt.Server.DTOs;

public record PurchaseRequest(bool ConfirmPayment);

public record PurchaseResponse(Guid GameId, string Title, decimal PricePaid);