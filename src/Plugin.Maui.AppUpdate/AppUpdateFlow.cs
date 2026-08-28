namespace Plugin.Maui.AppUpdate;

/// <summary>
/// How <see cref="IAppUpdate.StartAsync(System.Threading.CancellationToken)"/> will apply an update.
/// </summary>
public enum AppUpdateFlow
{
    /// <summary>
    /// No store or in-app flow is available.
    /// </summary>
    None = 0,

    /// <summary>
    /// Google Play immediate update (full-screen, blocking).
    /// </summary>
    Immediate = 1,

    /// <summary>
    /// Google Play flexible update (download in the background).
    /// </summary>
    Flexible = 2,

    /// <summary>
    /// Open the App Store or Play Store listing.
    /// </summary>
    Store = 3
}
