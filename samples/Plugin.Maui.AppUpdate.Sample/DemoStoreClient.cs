using Plugin.Maui.AppUpdate;

namespace Plugin.Maui.AppUpdate.Sample;

/// <summary>
/// In-app stand-in for Google Play / App Store so the sample works without a store listing.
/// </summary>
public sealed class DemoStoreClient : IStoreUpdateClient
{
    public StoreUpdateInfo Info { get; set; } = new()
    {
        IsAvailable = true,
        LatestVersion = "1.2.0",
        FlexibleAllowed = true,
        StoreUrl = "https://example.com/store",
        ReleaseNotes = "Store listing notes for 1.2.0."
    };

    public AppUpdateStartResult LastStart { get; private set; } =
        AppUpdateStartResult.From(AppUpdateStartStatus.Started, AppUpdateFlow.Flexible);

    public Task<StoreUpdateInfo> CheckAsync(AppUpdateOptions options, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Info);
    }

    public Task<AppUpdateStartResult> StartAsync(StoreUpdateInfo store, AppUpdateFlow flow, AppUpdateCheckResult check, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LastStart = AppUpdateStartResult.From(AppUpdateStartStatus.Started, flow, $"Demo started {flow} for {check.LatestVersion}.");
        return Task.FromResult(LastStart);
    }

    public Task<AppUpdateStartResult> CompleteFlexibleAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(AppUpdateStartResult.From(AppUpdateStartStatus.Started, AppUpdateFlow.Flexible, "Demo completed flexible update."));
    }

    public Task<StoreUpdateInfo?> ResumeAsync(AppUpdateOptions options, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<StoreUpdateInfo?>(null);
    }
}
