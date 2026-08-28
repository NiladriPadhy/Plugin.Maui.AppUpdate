namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Optional overrides for a single <see cref="IAppUpdate.StartAsync(System.Threading.CancellationToken)"/> call.
/// </summary>
public sealed class AppUpdateStartOptions
{
    /// <summary>
    /// Force a specific flow. When null, <see cref="AppUpdateCheckResult.Flow"/> is used.
    /// </summary>
    public AppUpdateFlow? Flow { get; set; }

    /// <summary>
    /// Start even if a recommended update is currently postponed.
    /// </summary>
    public bool IgnorePostponement { get; set; }
}
