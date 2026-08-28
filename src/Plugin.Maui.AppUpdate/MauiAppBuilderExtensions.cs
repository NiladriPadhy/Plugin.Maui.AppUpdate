using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Plugin.Maui.AppUpdate;

/// <summary>
/// MAUI host registration for in-app updates.
/// </summary>
public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Registers <see cref="IAppUpdate"/> and resumes pending Google Play updates on Android.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.UseMauiAppUpdate(options =>
    /// {
    ///     options.PolicyUri = new Uri("https://cdn.example.com/app-update.json");
    /// });
    /// </code>
    /// </example>
    public static MauiAppBuilder UseMauiAppUpdate(this MauiAppBuilder builder, Action<AppUpdateOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new AppUpdateOptions();
        configure?.Invoke(options);

        builder.Services.AddMauiAppUpdate(options);
        builder.Services.AddTransient<IMauiInitializeService, AppUpdateInitializer>();

        if (options.ResumePendingUpdates)
        {
            builder.ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                events.AddAndroid(android => android.OnResume(_ => ResumePending()));
#elif IOS
                events.AddiOS(ios => ios.OnActivated(_ => ResumePending()));
#endif
            });
        }

        return builder;
    }

    static void ResumePending()
    {
        var current = AppUpdate.Current;
        _ = Task.Run(async () =>
        {
            try
            {
                await current.ResumeAsync().ConfigureAwait(false);
            }
            catch
            {
                // Resume is best-effort; the next CheckAsync surfaces state.
            }
        });
    }
}
