namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Outcome of <see cref="IAppUpdate.StartAsync(System.Threading.CancellationToken)"/>.
/// </summary>
public enum AppUpdateStartStatus
{
    /// <summary>
    /// The platform update UI or store listing was opened.
    /// </summary>
    Started = 0,

    /// <summary>
    /// <see cref="IAppUpdate.CheckAsync"/> found nothing to start.
    /// </summary>
    NotAvailable = 1,

    /// <summary>
    /// A recommended update is still inside its postpone window.
    /// </summary>
    Postponed = 2,

    /// <summary>
    /// Maintenance is active; the store should not be opened.
    /// </summary>
    Maintenance = 3,

    /// <summary>
    /// The user dismissed a Play in-app update.
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// The platform flow failed to start.
    /// </summary>
    Failed = 5,

    /// <summary>
    /// A flexible update is already downloaded and ready to install.
    /// </summary>
    AlreadyDownloaded = 6,

    /// <summary>
    /// The public store listing was opened as a fallback.
    /// </summary>
    StoreOpened = 7
}
