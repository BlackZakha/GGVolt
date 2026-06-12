using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGVolt.Client.Services;

public interface IArchiveService
{
    event EventHandler<double>? ProgressChanged;
    Task ExtractZipAsync(string zipPath, string destinationPath, CancellationToken ct = default);
    Task DeleteZipAsync(string zipPath);
}