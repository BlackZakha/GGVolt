using System;
using System.Text.Json.Serialization;

namespace GGVolt.Client.Models.Api;

public record DownloadLinkResponse(
    [property: JsonPropertyName("url")] string SignedUrl,
    [property: JsonPropertyName("expiry")] TimeSpan Expiry,
    [property: JsonPropertyName("sizeBytes")] long SizeBytes
);