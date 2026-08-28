namespace Plugin.Maui.AppUpdate;

/// <summary>
/// Fetches an <see cref="AppUpdatePolicy"/> JSON document over HTTP.
/// </summary>
public sealed class HttpAppUpdatePolicyProvider : IAppUpdatePolicyProvider
{
    readonly HttpClient _http;
    readonly Uri _uri;
    readonly Action<HttpRequestMessage>? _configure;

    /// <summary>
    /// Creates a provider that GETs <paramref name="uri"/>.
    /// </summary>
    public HttpAppUpdatePolicyProvider(HttpClient httpClient, Uri uri, Action<HttpRequestMessage>? configureRequest = null)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _uri = uri ?? throw new ArgumentNullException(nameof(uri));
        _configure = configureRequest;
    }

    /// <inheritdoc />
    public async Task<AppUpdatePolicy?> FetchAsync(CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, _uri);
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _configure?.Invoke(message);

        using var response = await _http.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return await JsonSerializer
            .DeserializeAsync(stream, AppUpdateJsonContext.Default.AppUpdatePolicy, cancellationToken)
            .ConfigureAwait(false);
    }
}
