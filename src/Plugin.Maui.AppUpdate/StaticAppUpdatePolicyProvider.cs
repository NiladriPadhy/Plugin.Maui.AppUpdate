namespace Plugin.Maui.AppUpdate;

/// <summary>
/// In-memory policy for tests, samples, and fully local configuration.
/// </summary>
public sealed class StaticAppUpdatePolicyProvider : IAppUpdatePolicyProvider
{
    readonly Func<AppUpdatePolicy?> _factory;

    /// <summary>
    /// Serves a fixed policy on every fetch.
    /// </summary>
    public StaticAppUpdatePolicyProvider(AppUpdatePolicy? policy)
    {
        _factory = () => Clone(policy);
    }

    /// <summary>
    /// Serves a policy built from <paramref name="factory"/> on every fetch.
    /// </summary>
    public StaticAppUpdatePolicyProvider(Func<AppUpdatePolicy?> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <inheritdoc />
    public Task<AppUpdatePolicy?> FetchAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Clone(_factory()));
    }

    static AppUpdatePolicy? Clone(AppUpdatePolicy? policy)
    {
        if (policy is null)
        {
            return null;
        }

        return new AppUpdatePolicy
        {
            LatestVersion = policy.LatestVersion,
            MinimumSupportedVersion = policy.MinimumSupportedVersion,
            Priority = policy.Priority,
            ReleaseNotes = policy.ReleaseNotes,
            StoreUrl = policy.StoreUrl,
            AndroidStoreUrl = policy.AndroidStoreUrl,
            IosStoreUrl = policy.IosStoreUrl,
            PostponeDays = policy.PostponeDays,
            MaxPostponements = policy.MaxPostponements,
            Maintenance = policy.Maintenance is null
                ? null
                : new AppUpdateMaintenancePolicy
                {
                    Enabled = policy.Maintenance.Enabled,
                    Message = policy.Maintenance.Message,
                    StartsAt = policy.Maintenance.StartsAt,
                    EndsAt = policy.Maintenance.EndsAt
                }
        };
    }
}
