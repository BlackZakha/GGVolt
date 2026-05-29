using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace GGVolt.Infrastructure.Storage;

public class MinioFileStorageService : IFileStorageService, IDisposable
{
    private readonly IMinioClient _minio;
    private readonly MinioSettings _settings;
    private readonly ILogger<MinioFileStorageService> _logger;

    public MinioFileStorageService(IOptions<MinioSettings> options, ILogger<MinioFileStorageService> logger)
    {
        _settings = options.Value;
        _logger = logger;

        // MinIO SDK v6 возвращает IMinioClient
        _minio = new MinioClient()
            .WithEndpoint(_settings.Endpoint)
            .WithCredentials(_settings.AccessKey, _settings.SecretKey)
            .WithRegion(_settings.Region)
            .WithSSL(_settings.UseSsl)
            .Build();
    }

    public async Task<string> GeneratePresignedUrlAsync(string objectKey, TimeSpan expiry, CancellationToken ct = default)
    {
        try
        {
            if (!await FileExistsAsync(objectKey, ct))
                throw new FileNotFoundException($"Файл '{objectKey}' не найден в хранилище");

            // В SDK v6 CancellationToken не передаётся отдельным параметром
            var args = new PresignedGetObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectKey)
                .WithExpiry((int)expiry.TotalSeconds);

            return await _minio.PresignedGetObjectAsync(args);
        }
        catch (MinioException ex)
        {
            _logger.LogError(ex, "Ошибка генерации Pre-Signed URL для {ObjectKey}", objectKey);
            throw new InvalidOperationException("Не удалось создать ссылку для скачивания", ex);
        }
    }

    public async Task<bool> UploadFileAsync(string objectKey, Stream content, string sha256Hash, long size, IProgress<float>? progress = null, CancellationToken ct = default)
    {
        try
        {
            var bucketExists = await _minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(_settings.BucketName), ct);
            if (!bucketExists)
                await _minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(_settings.BucketName), ct);

            using var sha256 = SHA256.Create();
            var streamHash = await ComputeStreamHashAsync(content, sha256, progress, ct);

            if (!string.Equals(streamHash, sha256Hash, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Хэш файла не совпадает: ожидался {Expected}, получен {Actual}", sha256Hash, streamHash);
                return false;
            }

            content.Position = 0;

            var args = new PutObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectKey)
                .WithStreamData(content)
                .WithObjectSize(size)
                .WithContentType("application/octet-stream");

            await _minio.PutObjectAsync(args, ct);
            _logger.LogInformation("Файл {ObjectKey} успешно загружен ({Size} байт)", objectKey, size);
            return true;
        }
        catch (MinioException ex)
        {
            _logger.LogError(ex, "Ошибка загрузки файла {ObjectKey}", objectKey);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string objectKey, CancellationToken ct = default)
    {
        try
        {
            await _minio.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectKey), ct);
            return true;
        }
        catch (ObjectNotFoundException)
        {
            return false;
        }
        catch (MinioException ex)
        {
            _logger.LogWarning(ex, "Ошибка проверки существования {ObjectKey}", objectKey);
            return false;
        }
    }

    public async Task<bool> DeleteFileAsync(string objectKey, CancellationToken ct = default)
    {
        try
        {
            await _minio.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectKey), ct);
            _logger.LogInformation("Файл {ObjectKey} удалён", objectKey);
            return true;
        }
        catch (MinioException ex)
        {
            _logger.LogError(ex, "Ошибка удаления {ObjectKey}", objectKey);
            return false;
        }
    }

    public async Task<FileMetadata?> GetMetadataAsync(string objectKey, CancellationToken ct = default)
    {
        try
        {
            var stat = await _minio.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectKey), ct);

            return new FileMetadata(
                stat.Size,
                stat.ContentType ?? "application/octet-stream",
                stat.LastModified,
                stat.ETag);
        }
        catch (ObjectNotFoundException)
        {
            return null;
        }
        catch (MinioException ex)
        {
            _logger.LogWarning(ex, "Ошибка получения метаданных {ObjectKey}", objectKey);
            return null;
        }
    }

    private async Task<string> ComputeStreamHashAsync(Stream stream, SHA256 sha256, IProgress<float>? progress, CancellationToken ct)
    {
        var buffer = new byte[81920];
        long totalRead = 0;
        var originalPosition = stream.Position;

        while (true)
        {
            var read = await stream.ReadAsync(buffer, ct);
            if (read == 0) break;

            sha256.TransformBlock(buffer, 0, read, null, 0);
            totalRead += read;

            if (progress != null && stream.Length > 0)
                progress.Report((float)totalRead / stream.Length * 100);
        }

        sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        stream.Position = originalPosition;

        return Convert.ToHexString(sha256.Hash!);
    }

    public void Dispose() => (_minio as IDisposable)?.Dispose();
}