namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Snapshot returned by <see cref="IAppUpdate.CheckAsync"/>.
/// </summary>
public sealed class AppUpdateCheckResult
{
    /// <summary>
    /// <c>true</c> when an update should be offered now (not postponed).
    /// </summary>
    public bool IsAvailable { get; init; }

    /// <summary>
    /// <c>true</c> when the store or policy knows about a newer version, even if postponed.
    /// </summary>
    public bool HasUpdate { get; init; }

    /// <summary>
    /// The user must update before continuing.
    /// </summary>
    public bool IsMandatory { get; init; }

    /// <summary>
    /// The user may skip or postpone.
    /// </summary>
    public bool IsRecommended { get; init; }

    /// <summary>
    /// The app is inside an active maintenance window.
    /// </summary>
    public bool IsMaintenance { get; init; }

    /// <summary>
    /// A recommended update may be dismissed for a while.
    /// </summary>
    public bool CanPostpone { get; init; }

    /// <summary>
    /// A previous <see cref="IAppUpdate.PostponeAsync"/> is still in effect.
    /// </summary>
    public bool IsPostponed { get; init; }

    /// <summary>
    /// When a postponement expires, if one is active.
    /// </summary>
    public DateTimeOffset? PostponedUntil { get; init; }

    /// <summary>
    /// Resolved prompt strength.
    /// </summary>
    public AppUpdatePriority Priority { get; init; }

    /// <summary>
    /// How <see cref="IAppUpdate.StartAsync(System.Threading.CancellationToken)"/> will apply the update.
    /// </summary>
    public AppUpdateFlow Flow { get; init; }

    /// <summary>
    /// Why this result was produced.
    /// </summary>
    public AppUpdateReason Reason { get; init; }

    /// <summary>
    /// Installed marketing version.
    /// </summary>
    public string CurrentVersion { get; init; } = "";

    /// <summary>
    /// Newest version from the store or policy.
    /// </summary>
    public string? LatestVersion { get; init; }

    /// <summary>
    /// Policy floor that triggered a mandatory update, when applicable.
    /// </summary>
    public string? MinimumSupportedVersion { get; init; }

    /// <summary>
    /// What's new, from policy or the App Store lookup.
    /// </summary>
    public string? ReleaseNotes { get; init; }

    /// <summary>
    /// Store listing to open when the in-app flow is unavailable.
    /// </summary>
    public string? StoreUrl { get; init; }

    /// <summary>
    /// Google Play version code of the available package, when known.
    /// </summary>
    public int? AvailableVersionCode { get; init; }

    /// <summary>
    /// Play Console <c>inAppUpdatePriority</c> (0–5), when known.
    /// </summary>
    public int? StorePriority { get; init; }

    /// <summary>
    /// Days the installed version has been stale, when Play reports it.
    /// </summary>
    public int? StalenessDays { get; init; }

    /// <summary>
    /// Flexible-update install state from the last store query.
    /// </summary>
    public AppUpdateInstallStatus InstallStatus { get; init; }

    /// <summary>
    /// Resolved maintenance window, when the policy defined one.
    /// </summary>
    public MaintenanceInfo? Maintenance { get; init; }

    /// <summary>
    /// Optional human-readable detail (maintenance copy, store errors).
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Creates an up-to-date result for <paramref name="currentVersion"/>.
    /// </summary>
    public static AppUpdateCheckResult UpToDate(string currentVersion) => new()
    {
        CurrentVersion = currentVersion,
        Reason = AppUpdateReason.UpToDate,
        Priority = AppUpdatePriority.None,
        Flow = AppUpdateFlow.None
    };
}
