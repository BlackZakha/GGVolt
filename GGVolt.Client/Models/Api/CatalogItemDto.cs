using System.Text.Json.Serialization;

namespace GGVolt.Client.Models.Api;

public class CatalogItemDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("subtitle")] public string Subtitle { get; set; } = string.Empty;
    [JsonPropertyName("type")] public string Type { get; set; } = "game"; // game | software
    [JsonPropertyName("progress")] public double Progress { get; set; }
    [JsonPropertyName("action_text")] public string ActionText { get; set; } = "Скачать";
}