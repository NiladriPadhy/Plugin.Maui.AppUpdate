namespace Plugin.Maui.AppUpdate.Tests;

public sealed class EvaluatorTests
{
    static readonly DateTimeOffset Now = new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Up_to_date_when_store_and_policy_agree()
    {
        var result = Evaluate("1.2.3", StoreUpdateInfo.None, Harness.Policy(p => p.LatestVersion = "1.2.3"));

        Assert.False(result.IsAvailable);
        Assert.False(result.HasUpdate);
        Assert.Equal(AppUpdateReason.UpToDate, result.Reason);
    }

    [Fact]
    public void Store_update_is_recommended_by_default()
    {
        var result = Evaluate("1.0.0", Harness.StoreAvailable("1.1.0"));

        Assert.True(result.IsAvailable);
        Assert.True(result.IsRecommended);
        Assert.False(result.IsMandatory);
        Assert.True(result.CanPostpone);
        Assert.Equal(AppUpdateFlow.Flexible, result.Flow);
        Assert.Equal(AppUpdateReason.StoreUpdate, result.Reason);
        Assert.Equal("1.1.0", result.LatestVersion);
    }

    [Fact]
    public void Play_priority_four_is_mandatory_immediate()
    {
        var result = Evaluate("1.0.0", Harness.StoreAvailable("2.0.0", priority: 4));

        Assert.True(result.IsMandatory);
        Assert.False(result.CanPostpone);
        Assert.Equal(AppUpdateFlow.Immediate, result.Flow);
        Assert.Equal(AppUpdateReason.StoreUpdate, result.Reason);
    }

    [Fact]
    public void Below_minimum_supported_version_is_always_mandatory()
    {
        var result = Evaluate("1.0.0", StoreUpdateInfo.None, Harness.Policy(p =>
        {
            p.MinimumSupportedVersion = "2.0.0";
            p.LatestVersion = "2.1.0";
            p.ReleaseNotes = "Please update.";
        }));

        Assert.True(result.IsAvailable);
        Assert.True(result.IsMandatory);
        Assert.False(result.CanPostpone);
        Assert.Equal("2.0.0", result.MinimumSupportedVersion);
        Assert.Equal("Please update.", result.ReleaseNotes);
        Assert.Equal(AppUpdateReason.BelowMinimumVersion, result.Reason);
        Assert.Equal(AppUpdateFlow.Store, result.Flow);
    }

    [Fact]
    public void Policy_mandatory_wins_over_a_recommended_store_update()
    {
        var result = Evaluate("1.0.0", Harness.StoreAvailable("1.1.0"), Harness.Policy(p =>
        {
            p.Priority = "mandatory";
            p.ReleaseNotes = "Security fix.";
        }));

        Assert.True(result.IsMandatory);
        Assert.Equal(AppUpdateReason.PolicyMandatory, result.Reason);
        Assert.Equal("Security fix.", result.ReleaseNotes);
    }

    [Fact]
    public void Policy_none_does_not_prompt_even_when_the_store_has_an_update()
    {
        var result = Evaluate("1.0.0", Harness.StoreAvailable("2.0.0"), Harness.Policy(p => p.Priority = "none"));

        Assert.False(result.IsAvailable);
        Assert.True(result.HasUpdate);
        Assert.Equal(AppUpdatePriority.None, result.Priority);
    }

    [Fact]
    public void Maintenance_window_blocks_without_an_update()
    {
        var result = Evaluate("1.0.0", StoreUpdateInfo.None, Harness.Policy(p =>
        {
            p.Maintenance = new AppUpdateMaintenancePolicy
            {
                Enabled = true,
                Message = "Back at 18:00 UTC.",
                StartsAt = Now.AddHours(-1),
                EndsAt = Now.AddHours(6)
            };
        }));

        Assert.True(result.IsMaintenance);
        Assert.False(result.IsAvailable);
        Assert.Equal("Back at 18:00 UTC.", result.Maintenance?.Message);
        Assert.Equal(AppUpdateReason.Maintenance, result.Reason);
    }

    [Fact]
    public void Expired_maintenance_is_ignored()
    {
        var result = Evaluate("1.0.0", StoreUpdateInfo.None, Harness.Policy(p =>
        {
            p.Maintenance = new AppUpdateMaintenancePolicy
            {
                Enabled = true,
                Message = "Done",
                EndsAt = Now.AddMinutes(-1)
            };
        }));

        Assert.False(result.IsMaintenance);
        Assert.Equal(AppUpdateReason.UpToDate, result.Reason);
    }

    [Fact]
    public void Postponed_recommended_update_is_not_available()
    {
        var result = Evaluate(
            "1.0.0",
            Harness.StoreAvailable("1.2.0"),
            policy: null,
            postpone: new PostponeState
            {
                Version = "1.2.0",
                Count = 1,
                Until = Now.AddDays(2)
            });

        Assert.True(result.HasUpdate);
        Assert.True(result.IsPostponed);
        Assert.False(result.IsAvailable);
        Assert.False(result.CanPostpone);
        Assert.Equal(AppUpdateReason.Postponed, result.Reason);
    }

    [Fact]
    public void Postponement_does_not_apply_to_a_new_version()
    {
        var result = Evaluate(
            "1.0.0",
            Harness.StoreAvailable("1.3.0"),
            postpone: new PostponeState
            {
                Version = "1.2.0",
                Count = 2,
                Until = Now.AddDays(2)
            });

        Assert.True(result.IsAvailable);
        Assert.True(result.IsRecommended);
        Assert.True(result.CanPostpone);
    }

    [Fact]
    public void Release_notes_prefer_policy_over_the_store()
    {
        var store = new StoreUpdateInfo
        {
            IsAvailable = true,
            LatestVersion = "2.0.0",
            FlexibleAllowed = true,
            ReleaseNotes = "Store notes"
        };

        var result = Evaluate("1.0.0", store, Harness.Policy(p => p.ReleaseNotes = "Policy notes"));

        Assert.Equal("Policy notes", result.ReleaseNotes);
    }

    static AppUpdateCheckResult Evaluate(
        string current,
        StoreUpdateInfo store,
        AppUpdatePolicy? policy = null,
        PostponeState? postpone = null,
        AppUpdateOptions? options = null) =>
        AppUpdateEvaluator.Evaluate(current, store, policy, postpone, options ?? new AppUpdateOptions(), Now);
}
