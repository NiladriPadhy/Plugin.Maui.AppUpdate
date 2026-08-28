namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Resolved maintenance window from <see cref="AppUpdateMaintenancePolicy"/>.
/// </summary>
public sealed class MaintenanceInfo
{
    /// <summary>
    /// <c>true</c> when the window is enabled and <see cref="IAppUpdate.CheckAsync"/> ran inside it.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Message to show while the app is blocked.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Inclusive UTC start. <c>null</c> means already started.
    /// </summary>
    public DateTimeOffset? StartsAt { get; init; }

    /// <summary>
    /// Exclusive UTC end. <c>null</c> means open-ended.
    /// </summary>
    public DateTimeOffset? EndsAt { get; init; }
}
