using System.Text.Json.Serialization;

namespace GGVolt.Client.Models.Api;

public class LibraryItemDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("version")] public string Version { get; set; } = string.Empty;
    [JsonPropertyName("status_text")] public string StatusText { get; set; } = string.Empty;
    [JsonPropertyName("progress")] public double Progress { get; set; }
    [JsonPropertyName("action_text")] public string ActionText { get; set; } = "Запустить";
}