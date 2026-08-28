namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Optional maintenance window in a remote or local <see cref="AppUpdatePolicy"/>.
/// </summary>
public sealed class AppUpdateMaintenancePolicy
{
    /// <summary>
    /// When <c>true</c> and the current time is inside the window, <see cref="AppUpdateCheckResult.IsMaintenance"/> is set.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Message shown to the user while the app is unavailable.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Inclusive UTC start. Omit to start immediately.
    /// </summary>
    public DateTimeOffset? StartsAt { get; set; }

    /// <summary>
    /// Exclusive UTC end. Omit for an open-ended window.
    /// </summary>
    public DateTimeOffset? EndsAt { get; set; }
}
