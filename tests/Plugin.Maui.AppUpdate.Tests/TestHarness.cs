namespace Plugin.Maui.AppUpdate.Tests;

sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);

    public void Advance(TimeSpan duration) => UtcNow += duration;
}

sealed class FakeStoreClient : IStoreUpdateClient
{
    public StoreUpdateInfo Info { get; set; } = StoreUpdateInfo.None;

    public AppUpdateStartResult StartResult { get; set; } =
        AppUpdateStartResult.From(AppUpdateStartStatus.Started, AppUpdateFlow.Flexible);

    public AppUpdateStartResult CompleteResult { get; set; } =
        AppUpdateStartResult.From(AppUpdateStartStatus.Started, AppUpdateFlow.Flexible);

    public int StartCalls { get; private set; }

    public int CompleteCalls { get; private set; }

    public AppUpdateFlow? LastFlow { get; private set; }

    public Task<StoreUpdateInfo> CheckAsync(AppUpdateOptions options, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Info);
    }

    public Task<AppUpdateStartResult> StartAsync(StoreUpdateInfo store, AppUpdateFlow flow, AppUpdateCheckResult check, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        StartCalls++;
        LastFlow = flow;
        return Task.FromResult(StartResult);
    }

    public Task<AppUpdateStartResult> CompleteFlexibleAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CompleteCalls++;
        return Task.FromResult(CompleteResult);
    }

    public Task<StoreUpdateInfo?> ResumeAsync(AppUpdateOptions options, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<StoreUpdateInfo?>(null);
    }
}

static class Harness
{
    public static (AppUpdateImplementation Update, FakeClock Clock, FakeStoreClient Store, string Root) Create(
        Action<AppUpdateOptions>? configure = null,
        IAppUpdatePolicyProvider? policy = null)
    {
        var root = Directory.CreateTempSubdirectory("maui-appupdate-").FullName;
        var clock = new FakeClock();
        var store = new FakeStoreClient();
        var options = new AppUpdateOptions
        {
            StorageDirectory = root,
            CurrentVersion = "1.0.0",
            StoreClient = store,
            PolicyProvider = policy
        };
        configure?.Invoke(options);
        options.StoreClient = store;

        var postpone = new FilePostponeStore(StoragePath.Resolve(options));
        var update = AppUpdate.Create(options, clock, store, options.PolicyProvider, postpone);
        return (update, clock, store, root);
    }

    public static AppUpdatePolicy Policy(Action<AppUpdatePolicy>? configure = null)
    {
        var policy = new AppUpdatePolicy();
        configure?.Invoke(policy);
        return policy;
    }

    public static StoreUpdateInfo StoreAvailable(string version = "2.0.0", int? priority = null) => new()
    {
        IsAvailable = true,
        LatestVersion = version,
        Priority = priority,
        ImmediateAllowed = priority >= 4,
        FlexibleAllowed = true,
        StoreUrl = "https://example.com/store"
    };
}
