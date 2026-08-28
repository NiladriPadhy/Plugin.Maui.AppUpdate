namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Configuration for an <see cref="IAppUpdate"/> instance.
/// </summary>
public sealed class AppUpdateOptions
{
    /// <summary>
    /// Remote policy URL. When set and <see cref="PolicyProvider"/> is null, an HTTP provider is created.
    /// </summary>
    public Uri? PolicyUri { get; set; }

    /// <summary>
    /// Local fallback used when no remote policy is configured, or when the remote fetch fails.
    /// </summary>
    public AppUpdatePolicy? LocalPolicy { get; set; }

    /// <summary>
    /// Custom policy source. When set, <see cref="PolicyUri"/> is ignored.
    /// </summary>
    public IAppUpdatePolicyProvider? PolicyProvider { get; set; }

    /// <summary>
    /// Custom store adapter. Tests and samples inject a fake; Android and iOS supply a default.
    /// </summary>
    public IStoreUpdateClient? StoreClient { get; set; }

    /// <summary>
    /// ISO 3166-1 alpha-2 country for the iTunes lookup (for example <c>US</c>, <c>IN</c>).
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// iOS bundle id for the iTunes lookup. Defaults to the running app's package name.
    /// </summary>
    public string? BundleId { get; set; }

    /// <summary>
    /// Numeric App Store id, used to build a listing URL when the lookup has no <c>trackId</c>.
    /// </summary>
    public string? IosAppStoreId { get; set; }

    /// <summary>
    /// Android package name for a Play Store fallback URL. Defaults to the running app.
    /// </summary>
    public string? AndroidPackageName { get; set; }

    /// <summary>
    /// How long a recommended update stays dismissed.
    /// </summary>
    public TimeSpan PostponeDuration { get; set; } = AppUpdateDefaults.PostponeDuration;

    /// <summary>
    /// How many times the same latest version may be postponed.
    /// </summary>
    public int MaxPostponements { get; set; } = AppUpdateDefaults.MaxPostponements;

    /// <summary>
    /// Play <c>inAppUpdatePriority</c> at or above this value is treated as mandatory (default 4).
    /// </summary>
    public int ImmediatePriorityThreshold { get; set; } = AppUpdateDefaults.ImmediatePriorityThreshold;

    /// <summary>
    /// When Play In-App Update cannot start, open the public store listing instead.
    /// </summary>
    public bool OpenStoreWhenPlayUnavailable { get; set; } = true;

    /// <summary>
    /// Run <see cref="IAppUpdate.CheckAsync"/> once during MAUI startup.
    /// </summary>
    public bool CheckOnStart { get; set; }

    /// <summary>
    /// On Android resume, continue an interrupted immediate update or surface a downloaded flexible update.
    /// </summary>
    public bool ResumePendingUpdates { get; set; } = true;

    /// <summary>
    /// Override the installed marketing version. Tests set this; devices read it from the app.
    /// </summary>
    public string? CurrentVersion { get; set; }

    /// <summary>
    /// Override the persistence folder. Tests and custom hosts set this.
    /// </summary>
    public string? StorageDirectory { get; set; }

    /// <summary>
    /// Optional <see cref="HttpClient"/> for policy and iTunes requests. The plugin does not dispose a supplied client.
    /// </summary>
    public HttpClient? HttpClient { get; set; }

    /// <summary>
    /// Optional hook to add headers (for example authorization) to remote policy fetches.
    /// </summary>
    public Action<HttpRequestMessage>? ConfigureRequest { get; set; }

    /// <summary>
    /// HTTP timeout for the built-in policy provider and iTunes lookup.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = AppUpdateDefaults.RequestTimeout;
}
