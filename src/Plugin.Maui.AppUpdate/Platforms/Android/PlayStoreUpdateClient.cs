#if ANDROID
using Android.App;
using Microsoft.Maui.ApplicationModel;
using Xamarin.Google.Android.Play.Core.AppUpdate;
using Xamarin.Google.Android.Play.Core.AppUpdate.Install.Model;
using PlayAppUpdateOptions = Xamarin.Google.Android.Play.Core.AppUpdate.AppUpdateOptions;
using CancellationToken = System.Threading.CancellationToken;

namespace Plugin.Maui.AppUpdate;

sealed class PlayStoreUpdateClient : IStoreUpdateClient
{
    IAppUpdateManager? _manager;
    bool _openStoreWhenPlayUnavailable = true;

    IAppUpdateManager Manager =>
        _manager ??= AppUpdateManagerFactory.Create(Platform.CurrentActivity ?? Android.App.Application.Context);

    public async Task<StoreUpdateInfo> CheckAsync(AppUpdateOptions options, CancellationToken cancellationToken = default)
    {
        _openStoreWhenPlayUnavailable = options.OpenStoreWhenPlayUnavailable;
        try
        {
            var info = await GetInfoAsync(cancellationToken).ConfigureAwait(false);
            return Map(info, options, unavailable: false, error: null);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Map(null, options, unavailable: true, error: ex.Message);
        }
    }

    public async Task<AppUpdateStartResult> StartAsync(StoreUpdateInfo store, AppUpdateFlow flow, AppUpdateCheckResult check, CancellationToken cancellationToken = default)
    {
        if (flow == AppUpdateFlow.Store)
        {
            return _openStoreWhenPlayUnavailable
                ? await OpenListingAsync(check, store, cancellationToken).ConfigureAwait(false)
                : AppUpdateStartResult.From(AppUpdateStartStatus.Failed, flow, "Play In-App Update is unavailable.");
        }

        var activity = Platform.CurrentActivity;
        if (activity is null)
        {
            return await FallbackOrFail(check, store, flow, "No current Android activity.", cancellationToken).ConfigureAwait(false);
        }

        AppUpdateInfo? native = store.Native as AppUpdateInfo;
        native ??= await TryGetInfoAsync(cancellationToken).ConfigureAwait(false);
        if (native is null)
        {
            return await FallbackOrFail(check, store, flow, "Play did not return AppUpdateInfo.", cancellationToken).ConfigureAwait(false);
        }

        var type = flow == AppUpdateFlow.Immediate ? AppUpdateType.Immediate : AppUpdateType.Flexible;
        if (!native.IsUpdateTypeAllowed(type) && flow == AppUpdateFlow.Immediate && native.IsUpdateTypeAllowed(AppUpdateType.Flexible))
        {
            type = AppUpdateType.Flexible;
            flow = AppUpdateFlow.Flexible;
        }

        if (!native.IsUpdateTypeAllowed(type))
        {
            return await FallbackOrFail(check, store, flow, "Play does not allow the requested update type.", cancellationToken).ConfigureAwait(false);
        }

        try
        {
            var playOptions = PlayAppUpdateOptions.NewBuilder(type).Build();
            var started = Manager.StartUpdateFlowForResult(
                native,
                activity,
                playOptions,
                AppUpdateDefaults.PlayUpdateRequestCode);

            if (started)
            {
                return AppUpdateStartResult.From(AppUpdateStartStatus.Started, flow);
            }

            return await FallbackOrFail(check, store, flow, "Play StartUpdateFlowForResult returned false.", cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return await FallbackOrFail(check, store, flow, ex.Message, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<AppUpdateStartResult> CompleteFlexibleAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await AwaitPlayTask(Manager.CompleteUpdate(), cancellationToken).ConfigureAwait(false);
            return AppUpdateStartResult.From(AppUpdateStartStatus.Started, AppUpdateFlow.Flexible, "Completing flexible update.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return AppUpdateStartResult.From(AppUpdateStartStatus.Failed, AppUpdateFlow.Flexible, ex.Message);
        }
    }

    public async Task<StoreUpdateInfo?> ResumeAsync(AppUpdateOptions options, CancellationToken cancellationToken = default)
    {
        _openStoreWhenPlayUnavailable = options.OpenStoreWhenPlayUnavailable;
        try
        {
            var info = await GetInfoAsync(cancellationToken).ConfigureAwait(false);
            var mapped = Map(info, options, unavailable: false, error: null);
            if (mapped.DeveloperTriggeredInProgress || mapped.InstallStatus is AppUpdateInstallStatus.Downloaded or AppUpdateInstallStatus.Downloading)
            {
                return mapped;
            }

            return mapped.IsAvailable ? mapped : null;
        }
        catch
        {
            return null;
        }
    }

    async Task<AppUpdateInfo> GetInfoAsync(CancellationToken cancellationToken)
    {
        var result = await AwaitPlayTask(Manager.GetAppUpdateInfo(), cancellationToken).ConfigureAwait(false);
        return result as AppUpdateInfo
               ?? throw new AppUpdateException("Play AppUpdateInfo task returned no result.");
    }

    async Task<AppUpdateInfo?> TryGetInfoAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await GetInfoAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }

    async Task<AppUpdateStartResult> FallbackOrFail(AppUpdateCheckResult check, StoreUpdateInfo store, AppUpdateFlow flow, string message, CancellationToken cancellationToken)
    {
        if (_openStoreWhenPlayUnavailable && (check.IsMandatory || flow == AppUpdateFlow.Store))
        {
            var opened = await OpenListingAsync(check, store, cancellationToken).ConfigureAwait(false);
            if (opened.Started)
            {
                return opened;
            }
        }

        return AppUpdateStartResult.From(AppUpdateStartStatus.Failed, flow, message);
    }

    async Task<AppUpdateStartResult> OpenListingAsync(AppUpdateCheckResult check, StoreUpdateInfo store, CancellationToken cancellationToken)
    {
        var url = FirstNonEmpty(check.StoreUrl, store.StoreUrl);
        if (await StoreLauncher.OpenAsync(url, cancellationToken).ConfigureAwait(false))
        {
            return AppUpdateStartResult.From(AppUpdateStartStatus.StoreOpened, AppUpdateFlow.Store, url);
        }

        return AppUpdateStartResult.From(AppUpdateStartStatus.Failed, AppUpdateFlow.Store, "Unable to open the Play Store listing.");
    }

    StoreUpdateInfo Map(AppUpdateInfo? info, AppUpdateOptions options, bool unavailable, string? error)
    {
        var package = FirstNonEmpty(options.AndroidPackageName, new AppVersionProvider(options).GetPackageName());
        var listing = StoreLauncher.PlayStoreUrl(package);

        if (info is null)
        {
            return new StoreUpdateInfo
            {
                Unavailable = unavailable,
                Error = error,
                StoreUrl = listing
            };
        }

        var availability = info.UpdateAvailability();
        var install = MapInstallStatus(info.InstallStatus());
        int? staleness = null;
        try
        {
            var days = info.ClientVersionStalenessDays();
            if (days is not null)
            {
                staleness = days.IntValue();
            }
        }
        catch
        {
            // Some devices return null for staleness; the binding may throw.
        }

        return new StoreUpdateInfo
        {
            IsAvailable = availability == UpdateAvailability.UpdateAvailable
                          || availability == UpdateAvailability.DeveloperTriggeredUpdateInProgress,
            ImmediateAllowed = info.IsUpdateTypeAllowed(AppUpdateType.Immediate),
            FlexibleAllowed = info.IsUpdateTypeAllowed(AppUpdateType.Flexible),
            DeveloperTriggeredInProgress = availability == UpdateAvailability.DeveloperTriggeredUpdateInProgress,
            VersionCode = info.AvailableVersionCode(),
            Priority = info.UpdatePriority(),
            StalenessDays = staleness,
            StoreUrl = listing,
            InstallStatus = install,
            BytesDownloaded = info.BytesDownloaded(),
            TotalBytesToDownload = info.TotalBytesToDownload(),
            Unavailable = unavailable,
            Error = error,
            Native = info
        };
    }

    static AppUpdateInstallStatus MapInstallStatus(int status)
    {
        if (status == InstallStatus.Pending)
        {
            return AppUpdateInstallStatus.Pending;
        }

        if (status == InstallStatus.Downloading)
        {
            return AppUpdateInstallStatus.Downloading;
        }

        if (status == InstallStatus.Downloaded)
        {
            return AppUpdateInstallStatus.Downloaded;
        }

        if (status == InstallStatus.Installing)
        {
            return AppUpdateInstallStatus.Installing;
        }

        if (status == InstallStatus.Installed)
        {
            return AppUpdateInstallStatus.Installed;
        }

        if (status == InstallStatus.Failed)
        {
            return AppUpdateInstallStatus.Failed;
        }

        if (status == InstallStatus.Canceled)
        {
            return AppUpdateInstallStatus.Canceled;
        }

        return AppUpdateInstallStatus.Unknown;
    }

    static Task<Java.Lang.Object?> AwaitPlayTask(Android.Gms.Tasks.Task task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<Java.Lang.Object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        task.AddOnSuccessListener(new SuccessListener(result => tcs.TrySetResult(result)));
        task.AddOnFailureListener(new FailureListener(error => tcs.TrySetException(error)));
        if (cancellationToken.CanBeCanceled)
        {
            cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
        }

        return tcs.Task;
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

    sealed class SuccessListener(Action<Java.Lang.Object?> callback) : Java.Lang.Object, Android.Gms.Tasks.IOnSuccessListener
    {
        public void OnSuccess(Java.Lang.Object? result) => callback(result);
    }

    sealed class FailureListener(Action<Exception> callback) : Java.Lang.Object, Android.Gms.Tasks.IOnFailureListener
    {
        public void OnFailure(Java.Lang.Exception error) => callback(error);
    }
}
#endif
