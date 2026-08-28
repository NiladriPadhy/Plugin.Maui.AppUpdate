namespace Plugin.Maui.AppUpdate;

/// <summary>
/// How strongly the current check should prompt the user.
/// </summary>
public enum AppUpdatePriority
{
    /// <summary>
    /// No update prompt. Used for up-to-date apps and policy <c>none</c>.
    /// </summary>
    None = 0,

    /// <summary>
    /// The user may skip or postpone.
    /// </summary>
    Recommended = 1,

    /// <summary>
    /// The user must update before continuing.
    /// </summary>
    Mandatory = 2
}
