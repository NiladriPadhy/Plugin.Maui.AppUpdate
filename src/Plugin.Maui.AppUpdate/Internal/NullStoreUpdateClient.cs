namespace Plugin.Maui.AppUpdate;

sealed class NullStoreUpdateClient : IStoreUpdateClient
{
    public Task<StoreUpdateInfo> CheckAsync(AppUpdateOptions options, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(StoreUpdateInfo.None);
    }

    public async Task<AppUpdateStartResult> StartAsync(StoreUpdateInfo store, AppUpdateFlow flow, AppUpdateCheckResult check, CancellationToken cancellationToken = default)
    {
        var url = check.StoreUrl ?? store.StoreUrl;
        if (await StoreLauncher.OpenAsync(url, cancellationToken).ConfigureAwait(false))
        {
            return AppUpdateStartResult.From(AppUpdateStartStatus.StoreOpened, AppUpdateFlow.Store, url);
        }

        if (flow == AppUpdateFlow.Store || !string.IsNullOrWhiteSpace(url))
        {
            return AppUpdateStartResult.From(AppUpdateStartStatus.Failed, flow, "No store listing is available on this platform.");
        }

        return AppUpdateStartResult.From(AppUpdateStartStatus.Started, flow, "Store client is not available on net10.0.");
    }

    public Task<AppUpdateStartResult> CompleteFlexibleAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(AppUpdateStartResult.From(AppUpdateStartStatus.NotAvailable, AppUpdateFlow.None, "Flexible updates are Android-only."));
    }

    public Task<StoreUpdateInfo?> ResumeAsync(AppUpdateOptions options, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<StoreUpdateInfo?>(null);
    }
}
