using GGVolt.Core.Enums;

namespace GGVolt.Core.Entities;

public class Game : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ContentType ContentType { get; set; } = ContentType.Game;
    
    // Ссылки на контент
    public string CoverUrl { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty; // Ключ в S3/MinIO
    public string SystemRequirementsJson { get; set; } = "{}";

    public DateTime ReleaseDate { get; set; }
    public bool IsVisible { get; set; } = true;
    public long SizeBytes { get; set; }

    // Навигация
    public ICollection<License> Licenses { get; set; } = new List<License>();
}