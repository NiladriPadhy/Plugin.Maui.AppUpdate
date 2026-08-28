namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Registers in-app update services without MAUI lifecycle hooks.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="IAppUpdate"/> using the supplied options instance.
    /// </summary>
    public static IServiceCollection AddMauiAppUpdate(this IServiceCollection services, AppUpdateOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(options);
        services.TryAddSingleton<IAppUpdate>(sp =>
        {
            var resolved = sp.GetService<AppUpdateOptions>() ?? options;
            var update = AppUpdate.Create(resolved);
            AppUpdate.SetDefault(update);
            return update;
        });

        return services;
    }

    /// <summary>
    /// Adds <see cref="IAppUpdate"/> and applies <paramref name="configure"/> to a new options instance.
    /// </summary>
    public static IServiceCollection AddMauiAppUpdate(this IServiceCollection services, Action<AppUpdateOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new AppUpdateOptions();
        configure?.Invoke(options);
        return services.AddMauiAppUpdate(options);
    }
}
