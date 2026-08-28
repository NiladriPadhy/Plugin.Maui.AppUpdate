namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Entry point for in-app updates when dependency injection is not used.
/// </summary>
public static class AppUpdate
{
    static IAppUpdate? _current;

    /// <summary>
    /// Gets the shared <see cref="IAppUpdate"/> instance.
    /// </summary>
    public static IAppUpdate Current => _current ??= Create(new AppUpdateOptions());

    /// <summary>
    /// Queries the store and optional remote policy.
    /// </summary>
    /// <example>
    /// <code>
    /// var update = await AppUpdate.CheckAsync();
    /// if (update.IsAvailable)
    /// {
    ///     await AppUpdate.StartAsync();
    /// }
    /// </code>
    /// </example>
    public static Task<AppUpdateCheckResult> CheckAsync(CancellationToken cancellationToken = default) =>
        Current.CheckAsync(cancellationToken);

    /// <summary>
    /// Starts the flow from the last check, or runs <see cref="CheckAsync"/> first.
    /// </summary>
    public static Task<AppUpdateStartResult> StartAsync(CancellationToken cancellationToken = default) =>
        Current.StartAsync(cancellationToken);

    /// <summary>
    /// Creates an in-app update client with platform store adapters and optional HTTP policy.
    /// </summary>
    public static IAppUpdate Create(AppUpdateOptions? options = null)
    {
        options ??= new AppUpdateOptions();
        var directory = StoragePath.Resolve(options);
        var postpone = new FilePostponeStore(directory);
        var version = new AppVersionProvider(options);
        var (policy, ownedHttp) = ResolvePolicy(options);
#if IOS
        if (options.StoreClient is null && options.HttpClient is null && ownedHttp is null)
        {
            ownedHttp = new HttpClient { Timeout = options.RequestTimeout };
        }
#endif
        var store = options.StoreClient ?? CreateStoreClient(options, options.HttpClient ?? ownedHttp);
        return new AppUpdateImplementation(options, SystemClock.Instance, version, store, policy, postpone, ownedHttp);
    }

    /// <summary>
    /// Replaces the shared instance. Intended for tests and custom implementations.
    /// </summary>
    public static void SetDefault(IAppUpdate implementation) =>
        _current = implementation ?? throw new ArgumentNullException(nameof(implementation));

    internal static AppUpdateImplementation Create(
        AppUpdateOptions options,
        IClock clock,
        IStoreUpdateClient store,
        IAppUpdatePolicyProvider? policy,
        FilePostponeStore postpone) =>
        new(options, clock, new AppVersionProvider(options), store, policy, postpone, ownedHttp: null);

    static IStoreUpdateClient CreateStoreClient(AppUpdateOptions options, HttpClient? http)
    {
#if ANDROID
        return new PlayStoreUpdateClient();
#elif IOS
        return new AppStoreUpdateClient(http ?? new HttpClient { Timeout = options.RequestTimeout });
#else
        return new NullStoreUpdateClient();
#endif
    }

    static (IAppUpdatePolicyProvider? Provider, HttpClient? OwnedHttp) ResolvePolicy(AppUpdateOptions options)
    {
        if (options.PolicyProvider is not null)
        {
            return (options.PolicyProvider, null);
        }

        if (options.PolicyUri is null)
        {
            return (options.LocalPolicy is null ? null : new StaticAppUpdatePolicyProvider(options.LocalPolicy), null);
        }

        if (options.HttpClient is not null)
        {
            return (new HttpAppUpdatePolicyProvider(options.HttpClient, options.PolicyUri, options.ConfigureRequest), null);
        }

        var http = new HttpClient { Timeout = options.RequestTimeout };
        return (new HttpAppUpdatePolicyProvider(http, options.PolicyUri, options.ConfigureRequest), http);
    }
}
