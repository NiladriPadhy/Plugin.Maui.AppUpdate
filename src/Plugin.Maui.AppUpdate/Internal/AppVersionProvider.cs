#if ANDROID || IOS
using Microsoft.Maui.ApplicationModel;
#endif

namespace Plugin.Maui.AppUpdate;

sealed class AppVersionProvider
{
    readonly AppUpdateOptions _options;

    public AppVersionProvider(AppUpdateOptions options)
    {
        _options = options;
    }

    public string GetVersion()
    {
        if (!string.IsNullOrWhiteSpace(_options.CurrentVersion))
        {
            return _options.CurrentVersion.Trim();
        }

#if ANDROID || IOS
        try
        {
            var version = AppInfo.Current.VersionString;
            if (!string.IsNullOrWhiteSpace(version))
            {
                return version;
            }
        }
        catch
        {
            // Hosts without MAUI app context fall through.
        }
#endif

        return "0.0.0";
    }

    public string? GetPackageName()
    {
#if ANDROID || IOS
        try
        {
            return AppInfo.Current.PackageName;
        }
        catch
        {
            return null;
        }
#else
        return null;
#endif
    }
}
