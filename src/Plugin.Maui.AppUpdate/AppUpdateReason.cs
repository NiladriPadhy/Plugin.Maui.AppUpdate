namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Why <see cref="AppUpdateCheckResult"/> reached its current state.
/// </summary>
public enum AppUpdateReason
{
    /// <summary>
    /// Installed version satisfies the store and the policy.
    /// </summary>
    UpToDate = 0,

    /// <summary>
    /// The store reports a newer package.
    /// </summary>
    StoreUpdate = 1,

    /// <summary>
    /// Installed version is below <see cref="AppUpdatePolicy.MinimumSupportedVersion"/>.
    /// </summary>
    BelowMinimumVersion = 2,

    /// <summary>
    /// Remote or local policy marks the update as mandatory.
    /// </summary>
    PolicyMandatory = 3,

    /// <summary>
    /// Remote or local policy marks the update as recommended.
    /// </summary>
    PolicyRecommended = 4,

    /// <summary>
    /// The app is in a maintenance window.
    /// </summary>
    Maintenance = 5,

    /// <summary>
    /// A recommended update was postponed and the window has not expired.
    /// </summary>
    Postponed = 6,

    /// <summary>
    /// The store could not be queried; policy (if any) still applies.
    /// </summary>
    StoreUnavailable = 7
}
