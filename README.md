# Plugin.Maui.AppUpdate

[![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.AppUpdate.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.Maui.AppUpdate)

A cross-platform in-app update framework for **.NET MAUI** on **Android** and **iOS**.

```
var update = await appUpdate.CheckAsync();

if (update.IsAvailable)
{
    await appUpdate.StartAsync();
}
```

```
Remote policy (optional)
        ↓
Store check
  Android → Google Play In-App Update
  iOS     → App Store version lookup
        ↓
Mandatory / Recommended / Maintenance
        ↓
Start  or  Postpone
```

## Install

Package: [https://www.nuget.org/packages/Plugin.Maui.AppUpdate](https://www.nuget.org/packages/Plugin.Maui.AppUpdate)

```bash
dotnet add package Plugin.Maui.AppUpdate
```

Target frameworks: `net10.0`, `net10.0-android`, `net10.0-ios`.

## Quick start

```csharp
using Plugin.Maui.AppUpdate;

builder
    .UseMauiApp<App>()
    .UseMauiAppUpdate(options =>
    {
        options.PolicyUri = new Uri("https://cdn.example.com/app-update.json");
    });
```

Resolve `IAppUpdate` from dependency injection, or use `AppUpdate.Current`.

```csharp
var update = await appUpdate.CheckAsync();

if (update.IsMaintenance)
{
    // show update.Maintenance.Message and stop
    return;
}

if (update.IsAvailable)
{
    await appUpdate.StartAsync();
}
```

On **Android**, `StartAsync` launches a Google Play immediate or flexible update when the app was installed from Play. On **iOS**, it opens the App Store listing.

## What you get

| Capability | How |
| --- | --- |
| **Google Play In-App Update** | Immediate (blocking) and flexible (background) flows via Play Core. |
| **App Store version check** | iTunes lookup by bundle id; `StartAsync` opens the listing. |
| **Mandatory update** | Policy `priority: mandatory`, Play priority ≥ 4, or below the minimum version. |
| **Recommended update** | Default for a newer store/policy version the user may skip. |
| **Minimum supported version** | Installed version below `minimumSupportedVersion` is always mandatory. |
| **Release notes** | From the policy document, or the App Store lookup. |
| **Maintenance mode** | Policy window with a message; `StartAsync` does not open the store. |
| **Update postponement** | `PostponeAsync` hides a recommended update for N days (default 3, max 2 times). |
| **Resume** | Android resumes an interrupted immediate update and surfaces a downloaded flexible update. |

## Remote policy

Host a JSON document on any HTTPS CDN or API:

```json
{
  "latestVersion": "2.4.0",
  "minimumSupportedVersion": "2.0.0",
  "priority": "recommended",
  "releaseNotes": "Fixes a crash on login.",
  "androidStoreUrl": "https://play.google.com/store/apps/details?id=com.example.app",
  "iosStoreUrl": "https://apps.apple.com/app/id1234567890",
  "postponeDays": 3,
  "maxPostponements": 2,
  "maintenance": {
    "enabled": false,
    "message": "Scheduled maintenance until 18:00 UTC.",
    "startsAt": "2026-08-28T16:00:00Z",
    "endsAt": "2026-08-28T18:00:00Z"
  }
}
```

`priority` accepts `mandatory`, `recommended`, or `none`.

```csharp
options.PolicyUri = new Uri("https://cdn.example.com/app-update.json");
options.ConfigureRequest = request =>
{
    request.Headers.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
};
```

Use `LocalPolicy` or `StaticAppUpdatePolicyProvider` when you do not have a remote file yet.

## Check result

```csharp
var update = await appUpdate.CheckAsync();

update.IsAvailable;              // prompt now (not postponed)
update.HasUpdate;                // store/policy knows a newer version
update.IsMandatory;
update.IsRecommended;
update.IsMaintenance;
update.CanPostpone;
update.CurrentVersion;
update.LatestVersion;
update.MinimumSupportedVersion;
update.ReleaseNotes;
update.StoreUrl;
update.Flow;                     // Immediate, Flexible, or Store
```

```csharp
if (update.CanPostpone)
{
    await appUpdate.PostponeAsync(TimeSpan.FromDays(3));
}

if (update.InstallStatus == AppUpdateInstallStatus.Downloaded)
{
    await appUpdate.CompleteFlexibleUpdateAsync();
}
```

## Platform notes

| | Android | iOS | `net10.0` |
| --- | --- | --- | --- |
| Check / policy / postpone / maintenance | Yes | Yes | Yes (tests) |
| In-app install | Play immediate / flexible | No (opens App Store) | No |
| Store fallback URL | Play Store listing | App Store listing | Optional URL |
| Resume pending update | `OnResume` | n/a | n/a |

Play In-App Updates only appear for builds installed from Google Play (internal app sharing or a track). Sideloaded and emulator builds fall back to the public listing when `OpenStoreWhenPlayUnavailable` is `true`.

iOS looks up `https://itunes.apple.com/lookup?bundleId=...`. Set `CountryCode` if the app is not in the US store.

```xml
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
```

## Sample

`samples/Plugin.Maui.AppUpdate.Sample` drives recommended, mandatory, minimum-version, maintenance, and postpone scenarios against a demo store client.

```bash
dotnet build src/Plugin.Maui.AppUpdate/Plugin.Maui.AppUpdate.csproj
dotnet pack src/Plugin.Maui.AppUpdate/Plugin.Maui.AppUpdate.csproj -c Release -o artifacts
dotnet test tests/Plugin.Maui.AppUpdate.Tests/Plugin.Maui.AppUpdate.Tests.csproj
dotnet build samples/Plugin.Maui.AppUpdate.Sample/Plugin.Maui.AppUpdate.Sample.csproj -f net10.0-android
```

## Pack from source

```bash
dotnet pack src/Plugin.Maui.AppUpdate/Plugin.Maui.AppUpdate.csproj -c Release -o artifacts
```

The `.nupkg` is written to `artifacts/Plugin.Maui.AppUpdate.1.0.0.nupkg`.

## License

MIT

## Support

> If this plugin saved you a weekend of native plumbing, consider buying me a coffee.
> Your support keeps it maintained, documented, and free.

[![Buy Me A Coffee](https://img.shields.io/badge/Buy%20Me%20a%20Coffee-ffdd00?style=for-the-badge&logo=buy-me-a-coffee&logoColor=black)](https://buymeacoffee.com/npadhy)

This library stays open source. A coffee helps cover time for bug fixes, new features, and docs.
