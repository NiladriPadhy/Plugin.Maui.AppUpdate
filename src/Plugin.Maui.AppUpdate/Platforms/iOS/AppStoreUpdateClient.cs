#if IOS
namespace Plugin.Maui.AppUpdate;

sealed class AppStoreUpdateClient : IStoreUpdateClient
{
    readonly HttpClient _http;

    public AppStoreUpdateClient(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    public async Task<StoreUpdateInfo> CheckAsync(AppUpdateOptions options, CancellationToken cancellationToken = default)
    {
        var bundleId = FirstNonEmpty(options.BundleId, new AppVersionProvider(options).GetPackageName());
        if (string.IsNullOrWhiteSpace(bundleId))
        {
            return new StoreUpdateInfo
            {
                Unavailable = true,
                Error = "iOS bundle id is not available.",
                StoreUrl = StoreLauncher.AppStoreUrl(options.IosAppStoreId)
            };
        }

        var uri = BuildLookupUri(bundleId, options.CountryCode);

        try
        {
            using var response = await _http.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var lookup = await JsonSerializer
                .DeserializeAsync(stream, AppUpdateJsonContext.Default.ITunesLookupResponse, cancellationToken)
                .ConfigureAwait(false);

            var result = lookup?.Results.FirstOrDefault();
            if (result is null || string.IsNullOrWhiteSpace(result.Version))
            {
                return new StoreUpdateInfo
                {
                    Unavailable = true,
                    Error = "App Store lookup returned no version.",
                    StoreUrl = StoreLauncher.AppStoreUrl(options.IosAppStoreId)
                };
            }

            var current = new AppVersionProvider(options).GetVersion();
            var storeUrl = FirstNonEmpty(
                result.TrackViewUrl,
                StoreLauncher.AppStoreUrl(result.TrackId > 0 ? result.TrackId.ToString(CultureInfo.InvariantCulture) : options.IosAppStoreId));

            return new StoreUpdateInfo
            {
                IsAvailable = VersionComparer.IsNewerThan(result.Version, current),
                LatestVersion = result.Version,
                StoreUrl = storeUrl,
                ReleaseNotes = result.ReleaseNotes
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new StoreUpdateInfo
            {
                Unavailable = true,
                Error = ex.Message,
                StoreUrl = StoreLauncher.AppStoreUrl(options.IosAppStoreId)
            };
        }
    }

    public async Task<AppUpdateStartResult> StartAsync(StoreUpdateInfo store, AppUpdateFlow flow, AppUpdateCheckResult check, CancellationToken cancellationToken = default)
    {
        var url = FirstNonEmpty(check.StoreUrl, store.StoreUrl);
        if (await StoreLauncher.OpenAsync(url, cancellationToken).ConfigureAwait(false))
        {
            return AppUpdateStartResult.From(AppUpdateStartStatus.StoreOpened, AppUpdateFlow.Store, url);
        }

        return AppUpdateStartResult.From(AppUpdateStartStatus.Failed, AppUpdateFlow.Store, "Unable to open the App Store listing.");
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

    static Uri BuildLookupUri(string bundleId, string? country)
    {
        var url = "https://itunes.apple.com/lookup?bundleId=" + Uri.EscapeDataString(bundleId);
        if (!string.IsNullOrWhiteSpace(country))
        {
            url += "&country=" + Uri.EscapeDataString(country.Trim());
        }

        return new Uri(url);
    }

    static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}
#endif
