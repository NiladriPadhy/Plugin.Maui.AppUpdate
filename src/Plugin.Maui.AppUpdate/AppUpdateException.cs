namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Thrown when an in-app update check or start fails in a way the caller should see.
/// </summary>
public sealed class AppUpdateException : Exception
{
    /// <summary>
    /// Creates an exception with a human-readable message.
    /// </summary>
    public AppUpdateException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates an exception that wraps <paramref name="innerException"/>.
    /// </summary>
    public AppUpdateException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
