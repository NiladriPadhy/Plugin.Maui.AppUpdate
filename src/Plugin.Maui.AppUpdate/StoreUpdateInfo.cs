namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Raw store payload before policy and postponement are applied.
/// </summary>
public sealed class StoreUpdateInfo
{
    /// <summary>
    /// The store reports a newer package than the one installed.
    /// </summary>
    public bool IsAvailable { get; init; }

    /// <summary>
    /// Google Play allows an immediate (blocking) flow.
    /// </summary>
    public bool ImmediateAllowed { get; init; }

    /// <summary>
    /// Google Play allows a flexible (background) flow.
    /// </summary>
    public bool FlexibleAllowed { get; init; }

    /// <summary>
    /// An immediate update was interrupted and must be resumed.
    /// </summary>
    public bool DeveloperTriggeredInProgress { get; init; }

    /// <summary>
    /// Newest marketing version from the store, when known.
    /// </summary>
    public string? LatestVersion { get; init; }

    /// <summary>
    /// Play version code of the available package.
    /// </summary>
    public int? VersionCode { get; init; }

    /// <summary>
    /// Play Console priority 0–5.
    /// </summary>
    public int? Priority { get; init; }

    /// <summary>
    /// Days the installed version has been stale.
    /// </summary>
    public int? StalenessDays { get; init; }

    /// <summary>
    /// Public listing URL.
    /// </summary>
    public string? StoreUrl { get; init; }

    /// <summary>
    /// Store-provided release notes (App Store lookup).
    /// </summary>
    public string? ReleaseNotes { get; init; }

    /// <summary>
    /// Flexible-update install state.
    /// </summary>
    public AppUpdateInstallStatus InstallStatus { get; init; }

    /// <summary>
    /// Bytes copied so far.
    /// </summary>
    public long BytesDownloaded { get; init; }

    /// <summary>
    /// Total bytes, when known.
    /// </summary>
    public long TotalBytesToDownload { get; init; }

    /// <summary>
    /// <c>true</c> when the store API could not be reached (sideload, emulator, lookup failure).
    /// </summary>
    public bool Unavailable { get; init; }

    /// <summary>
    /// Optional store error for logs.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Platform-specific payload (Play <c>AppUpdateInfo</c>).
    /// </summary>
    public object? Native { get; init; }

    /// <summary>
    /// Empty store result used on <c>net10.0</c> and when the store is skipped.
    /// </summary>
    public static StoreUpdateInfo None { get; } = new();
}
