using System;
using System.Text.Json.Serialization;

namespace GGVolt.Client.Models.Api;

public record DownloadLinkResponse(
    [property: JsonPropertyName("signedUrl")] string SignedUrl,
    [property: JsonPropertyName("expiry")] TimeSpan Expiry,
    [property: JsonPropertyName("sizeBytes")] long SizeBytes
);