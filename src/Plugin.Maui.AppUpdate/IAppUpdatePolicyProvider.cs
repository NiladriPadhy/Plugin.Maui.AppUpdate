namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Supplies an <see cref="AppUpdatePolicy"/> document (usually HTTPS JSON).
/// </summary>
public interface IAppUpdatePolicyProvider
{
    /// <summary>
    /// Fetches the current policy. Return <c>null</c> to keep the last local policy only.
    /// </summary>
    Task<AppUpdatePolicy?> FetchAsync(CancellationToken cancellationToken = default);
}
