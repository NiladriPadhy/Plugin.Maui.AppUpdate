namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Outcome of <see cref="IAppUpdate.StartAsync(System.Threading.CancellationToken)"/> or <see cref="IAppUpdate.CompleteFlexibleUpdateAsync"/>.
/// </summary>
public sealed class AppUpdateStartResult
{
    /// <summary>
    /// <c>true</c> when a platform flow or store listing was opened.
    /// </summary>
    public bool Started { get; init; }

    /// <summary>
    /// Flow that was attempted.
    /// </summary>
    public AppUpdateFlow Flow { get; init; }

    /// <summary>
    /// Machine-readable outcome.
    /// </summary>
    public AppUpdateStartStatus Status { get; init; }

    /// <summary>
    /// Optional detail for logs or UI.
    /// </summary>
    public string? Message { get; init; }

    internal static AppUpdateStartResult From(AppUpdateStartStatus status, AppUpdateFlow flow, string? message = null) => new()
    {
        Started = status is AppUpdateStartStatus.Started or AppUpdateStartStatus.StoreOpened or AppUpdateStartStatus.AlreadyDownloaded,
        Flow = flow,
        Status = status,
        Message = message
    };
}
