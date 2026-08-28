namespace Plugin.Maui.AppUpdate;

sealed class AppUpdateImplementation : IAppUpdate
{
    readonly AppUpdateOptions _options;
    readonly IClock _clock;
    readonly AppVersionProvider _version;
    readonly IStoreUpdateClient _store;
    readonly IAppUpdatePolicyProvider? _policy;
    readonly FilePostponeStore _postpone;
    readonly HttpClient? _ownedHttp;
    readonly SemaphoreSlim _gate = new(1, 1);

    StoreUpdateInfo _lastStore = StoreUpdateInfo.None;
    AppUpdatePolicy? _lastPolicy;
    bool _disposed;

    public AppUpdateImplementation(
        AppUpdateOptions options,
        IClock clock,
        AppVersionProvider version,
        IStoreUpdateClient store,
        IAppUpdatePolicyProvider? policy,
        FilePostponeStore postpone,
        HttpClient? ownedHttp)
    {
        _options = options;
        _clock = clock;
        _version = version;
        _store = store;
        _policy = policy;
        _postpone = postpone;
        _ownedHttp = ownedHttp;
    }

    public bool IsSupported => true;

    public AppUpdateCheckResult? LastCheck { get; private set; }

    public event EventHandler<AppUpdateCheckResult>? Checked;

    public event EventHandler<AppUpdateProgressEventArgs>? ProgressChanged;

    public event EventHandler<AppUpdateCompletedEventArgs>? Completed;

    public async Task<AppUpdateCheckResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var current = _version.GetVersion();
            var policy = await LoadPolicyAsync(cancellationToken).ConfigureAwait(false);
            StoreUpdateInfo store;
            try
            {
                store = await _store.CheckAsync(_options, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                store = new StoreUpdateInfo
                {
                    Unavailable = true,
                    Error = ex.Message
                };
            }

            _lastStore = store;
            _lastPolicy = policy;
            var postpone = _postpone.Load();
            var result = AppUpdateEvaluator.Evaluate(current, store, policy, postpone, _options, _clock.UtcNow);
            LastCheck = result;
            RaiseProgress(store);
            Checked?.Invoke(this, result);
            return result;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<AppUpdateStartResult> StartAsync(CancellationToken cancellationToken = default)
    {
        var check = LastCheck ?? await CheckAsync(cancellationToken).ConfigureAwait(false);
        return await StartAsync(check, null, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AppUpdateStartResult> StartAsync(AppUpdateCheckResult check, AppUpdateStartOptions? options = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(check);

        if (check.IsMaintenance && !check.IsAvailable)
        {
            return Complete(AppUpdateStartResult.From(AppUpdateStartStatus.Maintenance, AppUpdateFlow.None, check.Maintenance?.Message));
        }

        if (check.IsPostponed && options?.IgnorePostponement != true)
        {
            return Complete(AppUpdateStartResult.From(AppUpdateStartStatus.Postponed, AppUpdateFlow.None, check.Message));
        }

        if (!check.IsAvailable && !check.HasUpdate && options?.IgnorePostponement != true)
        {
            return Complete(AppUpdateStartResult.From(AppUpdateStartStatus.NotAvailable, AppUpdateFlow.None));
        }

        var flow = options?.Flow ?? check.Flow;
        if (flow == AppUpdateFlow.None)
        {
            flow = string.IsNullOrWhiteSpace(check.StoreUrl) ? AppUpdateFlow.None : AppUpdateFlow.Store;
        }

        if (flow == AppUpdateFlow.None)
        {
            return Complete(AppUpdateStartResult.From(AppUpdateStartStatus.NotAvailable, AppUpdateFlow.None, "No update flow is available."));
        }

        if (check.InstallStatus == AppUpdateInstallStatus.Downloaded && flow == AppUpdateFlow.Flexible)
        {
            var completed = await _store.CompleteFlexibleAsync(cancellationToken).ConfigureAwait(false);
            return Complete(completed);
        }

        AppUpdateStartResult started;
        try
        {
            started = await _store.StartAsync(_lastStore, flow, check, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new AppUpdateException("Failed to start the in-app update.", ex);
        }

        if (started.Started)
        {
            _postpone.Clear();
        }

        return Complete(started);
    }

    public async Task PostponeAsync(TimeSpan? duration = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        var check = LastCheck ?? await CheckAsync(cancellationToken).ConfigureAwait(false);
        if (!check.CanPostpone && !check.IsRecommended)
        {
            throw new AppUpdateException("Only a recommended update can be postponed.");
        }

        var days = _lastPolicy?.PostponeDays;
        var window = duration
                     ?? (days is { } value && value > 0 ? TimeSpan.FromDays(value) : _options.PostponeDuration);
        if (window <= TimeSpan.Zero)
        {
            throw new AppUpdateException("Postpone duration must be greater than zero.");
        }

        var existing = _postpone.Load();
        var sameVersion = existing is not null
                          && string.Equals(existing.Version, check.LatestVersion, StringComparison.OrdinalIgnoreCase);
        var state = new PostponeState
        {
            Version = check.LatestVersion ?? check.CurrentVersion,
            Count = (sameVersion ? existing!.Count : 0) + 1,
            Until = _clock.UtcNow + window
        };

        var max = _lastPolicy?.MaxPostponements ?? _options.MaxPostponements;
        if (state.Count > max)
        {
            throw new AppUpdateException($"This version has already been postponed {max} time(s).");
        }

        _postpone.Save(state);
        LastCheck = AppUpdateEvaluator.Evaluate(
            check.CurrentVersion,
            _lastStore,
            _lastPolicy,
            state,
            _options,
            _clock.UtcNow);
    }

    public Task ClearPostponementAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        _postpone.Clear();
        return Task.CompletedTask;
    }

    public async Task<AppUpdateStartResult> CompleteFlexibleUpdateAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        try
        {
            var result = await _store.CompleteFlexibleAsync(cancellationToken).ConfigureAwait(false);
            return Complete(result);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new AppUpdateException("Failed to complete the flexible update.", ex);
        }
    }

    public async Task ResumeAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_options.ResumePendingUpdates)
        {
            return;
        }

        StoreUpdateInfo? resume;
        try
        {
            resume = await _store.ResumeAsync(_options, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return;
        }

        if (resume is null)
        {
            return;
        }

        _lastStore = resume;
        RaiseProgress(resume);

        if (resume.DeveloperTriggeredInProgress)
        {
            var check = LastCheck ?? AppUpdateCheckResult.UpToDate(_version.GetVersion());
            try
            {
                var started = await _store.StartAsync(resume, AppUpdateFlow.Immediate, check, cancellationToken).ConfigureAwait(false);
                Complete(started);
            }
            catch
            {
                // Resume is best-effort; the next CheckAsync will surface state.
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _ownedHttp?.Dispose();
        _gate.Dispose();
    }

    async Task<AppUpdatePolicy?> LoadPolicyAsync(CancellationToken cancellationToken)
    {
        if (_policy is null)
        {
            return _options.LocalPolicy;
        }

        try
        {
            return await _policy.FetchAsync(cancellationToken).ConfigureAwait(false) ?? _options.LocalPolicy;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return _options.LocalPolicy;
        }
    }

    void RaiseProgress(StoreUpdateInfo store)
    {
        if (store.InstallStatus is AppUpdateInstallStatus.Unknown or AppUpdateInstallStatus.Installed)
        {
            return;
        }

        ProgressChanged?.Invoke(this, new AppUpdateProgressEventArgs
        {
            Status = store.InstallStatus,
            BytesDownloaded = store.BytesDownloaded,
            TotalBytesToDownload = store.TotalBytesToDownload
        });
    }

    AppUpdateStartResult Complete(AppUpdateStartResult result)
    {
        Completed?.Invoke(this, new AppUpdateCompletedEventArgs { Result = result });
        return result;
    }
}
