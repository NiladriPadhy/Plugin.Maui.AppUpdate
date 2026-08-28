namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Cross-platform in-app updates for Android (Google Play) and iOS (App Store).
/// </summary>
public interface IAppUpdate : IDisposable
{
    /// <summary>
    /// Always <c>true</c> for Android, iOS, and the shared <c>net10.0</c> surface.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Last successful <see cref="CheckAsync"/> result, if any.
    /// </summary>
    AppUpdateCheckResult? LastCheck { get; }

    /// <summary>
    /// Raised after every <see cref="CheckAsync"/>.
    /// </summary>
    event EventHandler<AppUpdateCheckResult>? Checked;

    /// <summary>
    /// Raised when a flexible update reports download or install progress.
    /// </summary>
    event EventHandler<AppUpdateProgressEventArgs>? ProgressChanged;

    /// <summary>
    /// Raised when a start attempt finishes, is cancelled, or fails.
    /// </summary>
    event EventHandler<AppUpdateCompletedEventArgs>? Completed;

    /// <summary>
    /// Queries the store and optional remote policy, then applies postponement rules.
    /// </summary>
    /// <example>
    /// <code>
    /// var update = await appUpdate.CheckAsync();
    /// if (update.IsAvailable)
    /// {
    ///     await appUpdate.StartAsync();
    /// }
    /// </code>
    /// </example>
    Task<AppUpdateCheckResult> CheckAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts the flow from <see cref="LastCheck"/>, or runs <see cref="CheckAsync"/> first.
    /// </summary>
    Task<AppUpdateStartResult> StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts the flow described by <paramref name="check"/>.
    /// </summary>
    Task<AppUpdateStartResult> StartAsync(AppUpdateCheckResult check, AppUpdateStartOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dismisses a recommended update until the postpone window expires.
    /// Mandatory updates and maintenance cannot be postponed.
    /// </summary>
    Task PostponeAsync(TimeSpan? duration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears a stored postponement so the next check can prompt again.
    /// </summary>
    Task ClearPostponementAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Installs a downloaded Google Play flexible update. No-op on iOS.
    /// </summary>
    Task<AppUpdateStartResult> CompleteFlexibleUpdateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-reads Play state after the app returns to the foreground.
    /// </summary>
    Task ResumeAsync(CancellationToken cancellationToken = default);
}
