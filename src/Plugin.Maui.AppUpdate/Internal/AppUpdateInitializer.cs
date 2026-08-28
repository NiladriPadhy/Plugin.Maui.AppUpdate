using Microsoft.Maui.Hosting;

namespace Plugin.Maui.AppUpdate;

sealed class AppUpdateInitializer : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        var update = services.GetService<IAppUpdate>() ?? AppUpdate.Current;
        AppUpdate.SetDefault(update);

        var options = services.GetService<AppUpdateOptions>();
        if (options?.CheckOnStart != true)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await update.CheckAsync().ConfigureAwait(false);
            }
            catch
            {
                // Startup check is best-effort; the app can call CheckAsync later.
            }
        });
    }
}
