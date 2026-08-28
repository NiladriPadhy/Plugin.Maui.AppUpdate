namespace Plugin.Maui.AppUpdate.Tests;

public sealed class PolicyJsonTests
{
    [Fact]
    public void Policy_json_round_trips_camel_case()
    {
        const string json = """
            {
              "latestVersion": "2.4.0",
              "minimumSupportedVersion": "2.0.0",
              "priority": "mandatory",
              "releaseNotes": "Fixes a crash on login.",
              "postponeDays": 3,
              "maxPostponements": 2,
              "maintenance": {
                "enabled": true,
                "message": "Scheduled maintenance.",
                "endsAt": "2026-08-28T18:00:00+00:00"
              }
            }
            """;

        var policy = JsonSerializer.Deserialize(json, AppUpdateJsonContext.Default.AppUpdatePolicy);
        Assert.NotNull(policy);
        Assert.Equal("2.4.0", policy.LatestVersion);
        Assert.Equal("2.0.0", policy.MinimumSupportedVersion);
        Assert.Equal("mandatory", policy.Priority);
        Assert.Equal(3, policy.PostponeDays);
        Assert.True(policy.Maintenance?.Enabled);
        Assert.Equal("Scheduled maintenance.", policy.Maintenance?.Message);
    }

    [Fact]
    public async Task Http_provider_deserializes_a_policy_document()
    {
        var json = """{"latestVersion":"2.1.0","priority":"recommended","releaseNotes":"Bug fixes"}""";
        using var http = new HttpClient(new StubHandler(json)) { BaseAddress = new Uri("https://cdn.example.com") };
        var provider = new HttpAppUpdatePolicyProvider(http, new Uri("https://cdn.example.com/app-update.json"));

        var policy = await provider.FetchAsync();
        Assert.Equal("2.1.0", policy?.LatestVersion);
        Assert.Equal("recommended", policy?.Priority);
        Assert.Equal("Bug fixes", policy?.ReleaseNotes);
    }

    sealed class StubHandler(string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            });
    }
}
