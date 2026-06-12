using System;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using GGVolt.Client.Enums;

namespace GGVolt.Client.Models.Download;

public partial class DownloadItem : ObservableObject
{
    [ObservableProperty] private Guid _gameId;
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _version = string.Empty;
    [ObservableProperty] private string _downloadUrl = string.Empty;
    [ObservableProperty] private string _installPath = string.Empty;
    [ObservableProperty] private string _zipPath = string.Empty;
    [ObservableProperty] private long _totalBytes;
    [ObservableProperty] private long _downloadedBytes;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private DownloadStatus _status;
    [ObservableProperty] private string _statusText = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private double _speedMbps;
    [ObservableProperty] private TimeSpan _remainingTime;
    [ObservableProperty] private CancellationTokenSource? _cancellationTokenSource;

    public double Percentage => TotalBytes > 0 ? (double)DownloadedBytes / TotalBytes * 100 : 0;
    public string DownloadSpeedText => SpeedMbps > 0 ? $"{SpeedMbps:F1} MB/s" : "";
    public string RemainingTimeText => RemainingTime.TotalSeconds > 0 ?
        RemainingTime.TotalMinutes > 1 ? $"{(int)RemainingTime.TotalMinutes} мин" : $"{RemainingTime.Seconds} сек" : "";
}
