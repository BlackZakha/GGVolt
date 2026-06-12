using System;
using System.Text.Json.Serialization;

namespace GGVolt.Client.Models.Api;

public record GameDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("coverUrl")] string CoverUrl,
    [property: JsonPropertyName("releaseDate")] DateTime ReleaseDate,
    [property: JsonPropertyName("type")] ContentType Type
);

public enum ContentType
{
    Game = 1,
    Software = 2
}