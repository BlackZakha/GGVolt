using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GGVolt.Client.Models.Api;

public record GameDetailDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("coverUrl")] string CoverUrl,
    [property: JsonPropertyName("releaseDate")] DateTime ReleaseDate,
    [property: JsonPropertyName("type")] ContentType Type,
    [property: JsonPropertyName("screenshots")] List<string> Screenshots,
    [property: JsonPropertyName("systemRequirements")] SystemRequirementsDto? SystemRequirements
);

public record SystemRequirementsDto(
    [property: JsonPropertyName("minimum")] string Minimum,
    [property: JsonPropertyName("recommended")] string Recommended
);