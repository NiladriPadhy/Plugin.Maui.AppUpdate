namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Progress for a Google Play flexible update.
/// </summary>
public sealed class AppUpdateProgressEventArgs : EventArgs
{
    /// <summary>
    /// Current install state.
    /// </summary>
    public required AppUpdateInstallStatus Status { get; init; }

    /// <summary>
    /// Bytes copied so far.
    /// </summary>
    public long BytesDownloaded { get; init; }

    /// <summary>
    /// Total bytes, when known.
    /// </summary>
    public long TotalBytesToDownload { get; init; }

    /// <summary>
    /// 0–1 progress, or <c>null</c> when the total is unknown.
    /// </summary>
    public double? Percent =>
        TotalBytesToDownload > 0
            ? Math.Clamp(BytesDownloaded / (double)TotalBytesToDownload, 0, 1)
            : null;
}
