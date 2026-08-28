namespace Plugin.Maui.AppUpdate;

interface IClock
{
    DateTimeOffset UtcNow { get; }
}
