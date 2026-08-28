namespace Plugin.Maui.AppUpdate;

sealed class PostponeState
{
    public string? Version { get; set; }

    public int Count { get; set; }

    public DateTimeOffset? Until { get; set; }
}
