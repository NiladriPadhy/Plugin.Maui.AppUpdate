namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Platform store adapter. Android uses Google Play In-App Updates; iOS uses the App Store lookup.
/// </summary>
public interface IStoreUpdateClient
{
    /// <summary>
    /// Queries the store for a newer package.
    /// </summary>
    Task<StoreUpdateInfo> CheckAsync(AppUpdateOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts the in-app or store-listing flow.
    /// </summary>
    Task<AppUpdateStartResult> StartAsync(StoreUpdateInfo store, AppUpdateFlow flow, AppUpdateCheckResult check, CancellationToken cancellationToken = default);

    /// <summary>
    /// Installs a downloaded flexible update (Android). No-op on iOS.
    /// </summary>
    Task<AppUpdateStartResult> CompleteFlexibleAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-reads Play state on resume (interrupted immediate update, downloaded flexible update).
    /// </summary>
    Task<StoreUpdateInfo?> ResumeAsync(AppUpdateOptions options, CancellationToken cancellationToken = default);
}
