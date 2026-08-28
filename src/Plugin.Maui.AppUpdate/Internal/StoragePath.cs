#if ANDROID || IOS
using Microsoft.Maui.Storage;
#endif

namespace Plugin.Maui.AppUpdate;

static class StoragePath
{
    public static string Resolve(AppUpdateOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.StorageDirectory))
        {
            return options.StorageDirectory;
        }

        var root = TryAppData() ?? Path.Combine(Path.GetTempPath(), AppUpdateDefaults.StorageFolderName);
        return Path.Combine(root, AppUpdateDefaults.StorageFolderName);
    }

    static string? TryAppData()
    {
#if ANDROID || IOS
        try
        {
            return FileSystem.AppDataDirectory;
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
