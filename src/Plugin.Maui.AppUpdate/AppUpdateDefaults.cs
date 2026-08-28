namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Default values used by <see cref="AppUpdateOptions"/>.
/// </summary>
public static class AppUpdateDefaults
{
    /// <summary>
    /// Folder name under app data for postponement state.
    /// </summary>
    public const string StorageFolderName = "plugin.maui.appupdate";

    /// <summary>
    /// File name for persisted postponement.
    /// </summary>
    public const string PostponeFileName = "postpone.json";

    /// <summary>
    /// How long a recommended update stays dismissed.
    /// </summary>
    public static readonly TimeSpan PostponeDuration = TimeSpan.FromDays(3);

    /// <summary>
    /// How many times a given store version may be postponed.
    /// </summary>
    public const int MaxPostponements = 2;

    /// <summary>
    /// Google Play <c>inAppUpdatePriority</c> at or above this value is treated as mandatory.
    /// </summary>
    public const int ImmediatePriorityThreshold = 4;

    /// <summary>
    /// HTTP timeout for remote policy and the iTunes lookup.
    /// </summary>
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Activity request code used when starting a Play in-app update.
    /// </summary>
    public const int PlayUpdateRequestCode = 47001;
}
