#if ANDROID || IOS
using Microsoft.Maui.ApplicationModel;
#endif

namespace Plugin.Maui.AppUpdate;

static class StoreLauncher
{
    public static async Task<bool> OpenAsync(string? url, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

#if ANDROID || IOS
        return await Launcher.Default.OpenAsync(uri).ConfigureAwait(false);
#else
        return false;
#endif
    }

    public static string? PlayStoreUrl(string? packageName) =>
        string.IsNullOrWhiteSpace(packageName)
            ? null
            : "https://play.google.com/store/apps/details?id=" + packageName.Trim();

    public static string? AppStoreUrl(string? appStoreId) =>
        string.IsNullOrWhiteSpace(appStoreId)
            ? null
            : "https://apps.apple.com/app/id" + appStoreId.Trim();
}
