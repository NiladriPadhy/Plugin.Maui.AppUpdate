namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Install progress reported by Google Play flexible updates.
/// </summary>
public enum AppUpdateInstallStatus
{
    Unknown = 0,
    Pending = 1,
    Downloading = 2,
    Downloaded = 3,
    Installing = 4,
    Installed = 5,
    Failed = 6,
    Canceled = 7
}
