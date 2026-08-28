namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Server-driven update rules that sit on top of the store check.
/// </summary>
/// <remarks>
/// Host this as HTTPS JSON (camelCase). Combine it with Google Play / App Store
/// availability to force upgrades, show notes, or block the app during maintenance.
/// </remarks>
public sealed class AppUpdatePolicy
{
    /// <summary>
    /// Latest marketing version the policy knows about (for example <c>2.4.0</c>).
    /// </summary>
    public string? LatestVersion { get; set; }

    /// <summary>
    /// Inclusive floor. A lower installed version is always mandatory.
    /// </summary>
    public string? MinimumSupportedVersion { get; set; }

    /// <summary>
    /// <c>mandatory</c>, <c>recommended</c>, or <c>none</c>. When omitted, store signals decide.
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// What's new text shown with the prompt.
    /// </summary>
    public string? ReleaseNotes { get; set; }

    /// <summary>
    /// Fallback store URL used when a platform-specific URL is absent.
    /// </summary>
    public string? StoreUrl { get; set; }

    /// <summary>
    /// Play Store listing, used on Android when Play In-App Update is unavailable.
    /// </summary>
    public string? AndroidStoreUrl { get; set; }

    /// <summary>
    /// App Store listing, used on iOS when the iTunes lookup has no <c>trackViewUrl</c>.
    /// </summary>
    public string? IosStoreUrl { get; set; }

    /// <summary>
    /// How many days a recommended update stays dismissed. Overrides <see cref="AppUpdateOptions.PostponeDuration"/>.
    /// </summary>
    public int? PostponeDays { get; set; }

    /// <summary>
    /// How many times the same latest version may be postponed. Overrides <see cref="AppUpdateOptions.MaxPostponements"/>.
    /// </summary>
    public int? MaxPostponements { get; set; }

    /// <summary>
    /// Optional maintenance window.
    /// </summary>
    public AppUpdateMaintenancePolicy? Maintenance { get; set; }
}
