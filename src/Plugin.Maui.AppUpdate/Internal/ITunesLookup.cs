namespace Plugin.Maui.AppUpdate;

sealed class ITunesLookupResponse
{
    public int ResultCount { get; set; }

    public List<ITunesLookupResult> Results { get; set; } = [];
}

sealed class ITunesLookupResult
{
    public string? Version { get; set; }

    public long TrackId { get; set; }

    public string? TrackViewUrl { get; set; }

    public string? ReleaseNotes { get; set; }

    public string? CurrentVersionReleaseDate { get; set; }

    public string? BundleId { get; set; }
}
