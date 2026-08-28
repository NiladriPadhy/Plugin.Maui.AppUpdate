namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Raised when a store flow finishes, is cancelled, or fails.
/// </summary>
public sealed class AppUpdateCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Final status of the start attempt.
    /// </summary>
    public required AppUpdateStartResult Result { get; init; }
}
