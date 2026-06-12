using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace GGVolt.Client.Services;

public class ArchiveService : IArchiveService
{
    public event EventHandler<double>? ProgressChanged;

    public async Task ExtractZipAsync(string zipPath, string destinationPath, CancellationToken ct = default)
    {
        if (!File.Exists(zipPath))
            throw new FileNotFoundException("ZIP файл не найден", zipPath);

        Directory.CreateDirectory(destinationPath);

        using var archive = ZipFile.OpenRead(zipPath);
        var totalEntries = archive.Entries.Count;
        var processedEntries = 0;

        foreach (var entry in archive.Entries)
        {
            ct.ThrowIfCancellationRequested();

            // Пропускаем директории
            if (string.IsNullOrEmpty(entry.Name))
            {
                Directory.CreateDirectory(Path.Combine(destinationPath, entry.FullName));
                continue;
            }

            var destinationFilePath = Path.Combine(destinationPath, entry.FullName);
            var destinationDir = Path.GetDirectoryName(destinationFilePath);

            if (destinationDir != null)
                Directory.CreateDirectory(destinationDir);

            // Извлекаем файл
            entry.ExtractToFile(destinationFilePath, overwrite: true);

            processedEntries++;
            var progress = (double)processedEntries / totalEntries * 100;
            ProgressChanged?.Invoke(this, progress);

            // Даём UI обновиться
            await Task.Delay(1, ct);
        }
    }

    public Task DeleteZipAsync(string zipPath)
    {
        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }
        return Task.CompletedTask;
    }
}