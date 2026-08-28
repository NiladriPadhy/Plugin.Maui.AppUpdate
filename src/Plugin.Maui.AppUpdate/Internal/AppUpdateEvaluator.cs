namespace Plugin.Maui.AppUpdate;

static class AppUpdateEvaluator
{
    public static AppUpdateCheckResult Evaluate(
        string currentVersion,
        StoreUpdateInfo store,
        AppUpdatePolicy? policy,
        PostponeState? postpone,
        AppUpdateOptions options,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currentVersion);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(options);

        var maintenance = ResolveMaintenance(policy?.Maintenance, now);
        var latestVersion = FirstNonEmpty(store.LatestVersion, policy?.LatestVersion);
        var storeUrl = ResolveStoreUrl(store, policy);
        var releaseNotes = FirstNonEmpty(policy?.ReleaseNotes, store.ReleaseNotes);
        var belowMin = !string.IsNullOrWhiteSpace(policy?.MinimumSupportedVersion)
                       && !VersionComparer.IsAtLeast(currentVersion, policy.MinimumSupportedVersion);
        var storeNewer = store.IsAvailable || VersionComparer.IsNewerThan(store.LatestVersion, currentVersion);
        var policyNewer = VersionComparer.IsNewerThan(policy?.LatestVersion, currentVersion);
        var discoveredUpdate = belowMin || storeNewer || policyNewer;
        var policyPriority = ParsePriority(policy?.Priority);
        var storeMandatory = store.Priority is { } play
                             && play >= Math.Max(0, options.ImmediatePriorityThreshold);
        var hasUpdate = discoveredUpdate && (belowMin || policyPriority != AppUpdatePriority.None);

        var (priority, reason) = ResolvePriority(
            hasUpdate,
            belowMin,
            policyPriority,
            storeMandatory,
            storeNewer,
            policyNewer,
            store.Unavailable);

        if (!hasUpdate && maintenance is { IsActive: true })
        {
            return new AppUpdateCheckResult
            {
                HasUpdate = discoveredUpdate,
                IsMaintenance = true,
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion,
                MinimumSupportedVersion = policy?.MinimumSupportedVersion,
                ReleaseNotes = releaseNotes,
                StoreUrl = storeUrl,
                AvailableVersionCode = store.VersionCode,
                StorePriority = store.Priority,
                StalenessDays = store.StalenessDays,
                InstallStatus = store.InstallStatus,
                Maintenance = maintenance,
                Message = maintenance.Message,
                Reason = AppUpdateReason.Maintenance,
                Priority = AppUpdatePriority.None,
                Flow = AppUpdateFlow.None
            };
        }

        if (!hasUpdate)
        {
            return new AppUpdateCheckResult
            {
                HasUpdate = discoveredUpdate,
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion,
                MinimumSupportedVersion = policy?.MinimumSupportedVersion,
                ReleaseNotes = releaseNotes,
                StoreUrl = storeUrl,
                AvailableVersionCode = store.VersionCode,
                StorePriority = store.Priority,
                StalenessDays = store.StalenessDays,
                InstallStatus = store.InstallStatus,
                Maintenance = maintenance,
                IsMaintenance = maintenance?.IsActive == true,
                Message = store.Error,
                Reason = store.Unavailable
                    ? AppUpdateReason.StoreUnavailable
                    : discoveredUpdate
                        ? AppUpdateReason.StoreUpdate
                        : AppUpdateReason.UpToDate,
                Priority = AppUpdatePriority.None,
                Flow = AppUpdateFlow.None
            };
        }

        var maxPostponements = policy?.MaxPostponements ?? options.MaxPostponements;
        var postponeDuration = policy?.PostponeDays is { } days && days > 0
            ? TimeSpan.FromDays(days)
            : options.PostponeDuration;
        var postponeMatches = postpone is not null
                              && string.Equals(postpone.Version, latestVersion, StringComparison.OrdinalIgnoreCase);
        var postponedUntil = postponeMatches ? postpone!.Until : null;
        var isPostponed = priority == AppUpdatePriority.Recommended
                          && postponeMatches
                          && postponedUntil is { } until
                          && until > now
                          && postpone!.Count <= maxPostponements;
        var canPostpone = priority == AppUpdatePriority.Recommended
                          && !isPostponed
                          && (postponeMatches ? postpone!.Count : 0) < maxPostponements
                          && postponeDuration > TimeSpan.Zero;
        var isAvailable = !isPostponed;
        var flow = ResolveFlow(priority, store);

        return new AppUpdateCheckResult
        {
            IsAvailable = isAvailable,
            HasUpdate = true,
            IsMandatory = priority == AppUpdatePriority.Mandatory,
            IsRecommended = priority == AppUpdatePriority.Recommended,
            IsMaintenance = maintenance?.IsActive == true,
            CanPostpone = canPostpone,
            IsPostponed = isPostponed,
            PostponedUntil = isPostponed ? postponedUntil : null,
            Priority = priority,
            Flow = isAvailable ? flow : AppUpdateFlow.None,
            Reason = isPostponed ? AppUpdateReason.Postponed : reason,
            CurrentVersion = currentVersion,
            LatestVersion = latestVersion,
            MinimumSupportedVersion = policy?.MinimumSupportedVersion,
            ReleaseNotes = releaseNotes,
            StoreUrl = storeUrl,
            AvailableVersionCode = store.VersionCode,
            StorePriority = store.Priority,
            StalenessDays = store.StalenessDays,
            InstallStatus = store.InstallStatus,
            Maintenance = maintenance,
            Message = isPostponed
                ? $"Recommended update postponed until {postponedUntil:u}."
                : maintenance?.IsActive == true
                    ? maintenance.Message
                    : store.Error
        };
    }

    public static AppUpdatePriority? ParsePriority(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "mandatory" or "required" or "force" or "forced" or "immediate" => AppUpdatePriority.Mandatory,
            "recommended" or "flexible" or "optional" or "soft" => AppUpdatePriority.Recommended,
            "none" or "off" or "disabled" or "skip" => AppUpdatePriority.None,
            _ => null
        };
    }

    static (AppUpdatePriority Priority, AppUpdateReason Reason) ResolvePriority(
        bool hasUpdate,
        bool belowMin,
        AppUpdatePriority? policyPriority,
        bool storeMandatory,
        bool storeNewer,
        bool policyNewer,
        bool storeUnavailable)
    {
        if (belowMin)
        {
            return (AppUpdatePriority.Mandatory, AppUpdateReason.BelowMinimumVersion);
        }

        if (policyPriority == AppUpdatePriority.None)
        {
            return storeNewer
                ? (AppUpdatePriority.None, storeUnavailable ? AppUpdateReason.StoreUnavailable : AppUpdateReason.StoreUpdate)
                : (AppUpdatePriority.None, AppUpdateReason.UpToDate);
        }

        if (policyPriority == AppUpdatePriority.Mandatory && hasUpdate)
        {
            return (AppUpdatePriority.Mandatory, AppUpdateReason.PolicyMandatory);
        }

        if (storeMandatory && storeNewer)
        {
            return (AppUpdatePriority.Mandatory, AppUpdateReason.StoreUpdate);
        }

        if (policyPriority == AppUpdatePriority.Recommended && hasUpdate)
        {
            return (AppUpdatePriority.Recommended, AppUpdateReason.PolicyRecommended);
        }

        if (storeNewer || policyNewer)
        {
            return (AppUpdatePriority.Recommended, storeNewer ? AppUpdateReason.StoreUpdate : AppUpdateReason.PolicyRecommended);
        }

        return (AppUpdatePriority.None, AppUpdateReason.UpToDate);
    }

    static AppUpdateFlow ResolveFlow(AppUpdatePriority priority, StoreUpdateInfo store)
    {
        if (priority == AppUpdatePriority.None)
        {
            return AppUpdateFlow.None;
        }

        if (store.DeveloperTriggeredInProgress && store.ImmediateAllowed)
        {
            return AppUpdateFlow.Immediate;
        }

        if (priority == AppUpdatePriority.Mandatory && store.ImmediateAllowed)
        {
            return AppUpdateFlow.Immediate;
        }

        if (store.FlexibleAllowed)
        {
            return AppUpdateFlow.Flexible;
        }

        if (store.ImmediateAllowed)
        {
            return AppUpdateFlow.Immediate;
        }

        if (!string.IsNullOrWhiteSpace(store.StoreUrl) || store.Unavailable || !store.IsAvailable)
        {
            return AppUpdateFlow.Store;
        }

        return AppUpdateFlow.Store;
    }

    static MaintenanceInfo? ResolveMaintenance(AppUpdateMaintenancePolicy? policy, DateTimeOffset now)
    {
        if (policy is null)
        {
            return null;
        }

        var started = policy.StartsAt is null || now >= policy.StartsAt;
        var notEnded = policy.EndsAt is null || now < policy.EndsAt;
        var active = policy.Enabled && started && notEnded;

        return new MaintenanceInfo
        {
            IsActive = active,
            Message = policy.Message,
            StartsAt = policy.StartsAt,
            EndsAt = policy.EndsAt
        };
    }

    static string? ResolveStoreUrl(StoreUpdateInfo store, AppUpdatePolicy? policy)
    {
        if (!string.IsNullOrWhiteSpace(store.StoreUrl))
        {
            return store.StoreUrl;
        }

#if ANDROID
        return FirstNonEmpty(policy?.AndroidStoreUrl, policy?.StoreUrl);
#elif IOS
        return FirstNonEmpty(policy?.IosStoreUrl, policy?.StoreUrl);
#else
        return FirstNonEmpty(policy?.StoreUrl, policy?.AndroidStoreUrl, policy?.IosStoreUrl);
#endif
    }

    static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}
