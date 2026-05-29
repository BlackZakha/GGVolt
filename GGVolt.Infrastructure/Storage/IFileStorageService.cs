namespace GGVolt.Infrastructure.Storage;

public interface IFileStorageService
{
    /// <summary>Генерация временной ссылки на скачивание</summary>
    Task<string> GeneratePresignedUrlAsync(string objectKey, TimeSpan expiry, CancellationToken ct = default);
    
    /// <summary>Загрузка файла с валидацией хэша</summary>
    Task<bool> UploadFileAsync(string objectKey, Stream content, string sha256Hash, long size, IProgress<float>? progress = null, CancellationToken ct = default);
    
    /// <summary>Проверка существования файла</summary>
    Task<bool> FileExistsAsync(string objectKey, CancellationToken ct = default);
    
    /// <summary>Удаление файла</summary>
    Task<bool> DeleteFileAsync(string objectKey, CancellationToken ct = default);
    
    /// <summary>Получение метаданных файла</summary>
    Task<FileMetadata?> GetMetadataAsync(string objectKey, CancellationToken ct = default);
}

public record FileMetadata(long Size, string ContentType, DateTime LastModified, string ETag);