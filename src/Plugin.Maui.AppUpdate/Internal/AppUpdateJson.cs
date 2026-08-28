namespace Plugin.Maui.AppUpdate;

[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(AppUpdatePolicy))]
[JsonSerializable(typeof(AppUpdateMaintenancePolicy))]
[JsonSerializable(typeof(PostponeState))]
[JsonSerializable(typeof(ITunesLookupResponse))]
sealed partial class AppUpdateJsonContext : JsonSerializerContext;
