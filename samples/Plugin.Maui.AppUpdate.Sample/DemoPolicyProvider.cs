using Plugin.Maui.AppUpdate;

namespace Plugin.Maui.AppUpdate.Sample;

/// <summary>
/// Swappable remote-policy stand-in used by the sample buttons.
/// </summary>
public sealed class DemoPolicyProvider : IAppUpdatePolicyProvider
{
    public AppUpdatePolicy? Policy { get; set; } = Recommended();

    public Task<AppUpdatePolicy?> FetchAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Policy);
    }

    public static AppUpdatePolicy Recommended() => new()
    {
        LatestVersion = "1.2.0",
        Priority = "recommended",
        ReleaseNotes = "Faster startup and a new settings page.",
        PostponeDays = 3,
        MaxPostponements = 2
    };

    public static AppUpdatePolicy Mandatory() => new()
    {
        LatestVersion = "2.0.0",
        Priority = "mandatory",
        ReleaseNotes = "Required security update."
    };

    public static AppUpdatePolicy MinimumVersion() => new()
    {
        LatestVersion = "2.1.0",
        MinimumSupportedVersion = "2.0.0",
        ReleaseNotes = "This build is no longer supported."
    };

    public static AppUpdatePolicy Maintenance() => new()
    {
        Maintenance = new AppUpdateMaintenancePolicy
        {
            Enabled = true,
            Message = "Scheduled maintenance until 18:00 UTC."
        }
    };
}
