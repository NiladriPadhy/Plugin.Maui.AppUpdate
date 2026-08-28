using Microsoft.Extensions.Logging;
using Plugin.Maui.AppUpdate;

namespace Plugin.Maui.AppUpdate.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        var store = new DemoStoreClient();
        var policy = new DemoPolicyProvider();

        builder.Services.AddSingleton(store);
        builder.Services.AddSingleton(policy);
        builder.Services.AddSingleton<MainPage>();

        builder
            .UseMauiApp<App>()
            .UseMauiAppUpdate(options =>
            {
                options.CurrentVersion = "1.0.0";
                options.StoreClient = store;
                options.PolicyProvider = policy;
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
