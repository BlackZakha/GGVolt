using System;
using System.Text.Json.Serialization;

namespace GGVolt.Client.Models.Api;

public record PurchaseRequest(
    [property: JsonPropertyName("confirmPayment")] bool ConfirmPayment
);

public record PurchaseResponse(
    [property: JsonPropertyName("gameId")] Guid GameId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("pricePaid")] decimal PricePaid
);