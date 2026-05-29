namespace GGVolt.Client.Models;

public class CatalogItem
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public double Progress { get; set; } // 0–100
    public string ActionText { get; set; } = "Скачать";

    // Вычисляемое свойство для UI
    public bool IsDownloading => Progress > 0 && Progress < 100;
}