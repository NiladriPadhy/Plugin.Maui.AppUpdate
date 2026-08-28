namespace Plugin.Maui.AppUpdate.Tests;

public sealed class AppUpdateTests
{
    [Fact]
    public async Task CheckAsync_and_StartAsync_follow_the_common_api()
    {
        var (update, _, store, _) = Harness.Create();
        store.Info = Harness.StoreAvailable("1.4.0");

        var check = await update.CheckAsync();
        Assert.True(check.IsAvailable);

        var started = await update.StartAsync();
        Assert.True(started.Started);
        Assert.Equal(1, store.StartCalls);
        Assert.Equal(AppUpdateFlow.Flexible, store.LastFlow);
    }

    [Fact]
    public async Task Mandatory_policy_starts_immediate_when_Play_allows_it()
    {
        var (update, _, store, _) = Harness.Create(options =>
        {
            options.PolicyProvider = new StaticAppUpdatePolicyProvider(Harness.Policy(p =>
            {
                p.Priority = "mandatory";
                p.LatestVersion = "2.0.0";
            }));
        });
        store.Info = new StoreUpdateInfo
        {
            IsAvailable = true,
            LatestVersion = "2.0.0",
            ImmediateAllowed = true,
            FlexibleAllowed = true
        };

        var check = await update.CheckAsync();
        Assert.True(check.IsMandatory);
        Assert.Equal(AppUpdateFlow.Immediate, check.Flow);

        await update.StartAsync();
        Assert.Equal(AppUpdateFlow.Immediate, store.LastFlow);
    }

    [Fact]
    public async Task PostponeAsync_hides_a_recommended_update_until_the_window_expires()
    {
        var (update, clock, store, _) = Harness.Create();
        store.Info = Harness.StoreAvailable("1.5.0");

        Assert.True((await update.CheckAsync()).IsAvailable);
        await update.PostponeAsync(TimeSpan.FromDays(3));

        var postponed = await update.CheckAsync();
        Assert.True(postponed.IsPostponed);
        Assert.False(postponed.IsAvailable);

        var start = await update.StartAsync();
        Assert.Equal(AppUpdateStartStatus.Postponed, start.Status);
        Assert.Equal(0, store.StartCalls);

        clock.Advance(TimeSpan.FromDays(3).Add(TimeSpan.FromMinutes(1)));
        var again = await update.CheckAsync();
        Assert.True(again.IsAvailable);
        Assert.False(again.IsPostponed);
    }

    [Fact]
    public async Task Mandatory_update_cannot_be_postponed()
    {
        var (update, _, store, _) = Harness.Create(options =>
        {
            options.LocalPolicy = Harness.Policy(p => p.MinimumSupportedVersion = "2.0.0");
        });
        store.Info = StoreUpdateInfo.None;

        var check = await update.CheckAsync();
        Assert.True(check.IsMandatory);
        Assert.False(check.CanPostpone);

        await Assert.ThrowsAsync<AppUpdateException>(() => update.PostponeAsync());
    }

    [Fact]
    public async Task Maintenance_start_does_not_open_the_store()
    {
        var (update, _, store, _) = Harness.Create(options =>
        {
            options.PolicyProvider = new StaticAppUpdatePolicyProvider(Harness.Policy(p =>
            {
                p.Maintenance = new AppUpdateMaintenancePolicy
                {
                    Enabled = true,
                    Message = "We'll be back shortly."
                };
            }));
        });

        var check = await update.CheckAsync();
        Assert.True(check.IsMaintenance);
        Assert.False(check.IsAvailable);

        var start = await update.StartAsync();
        Assert.Equal(AppUpdateStartStatus.Maintenance, start.Status);
        Assert.Equal(0, store.StartCalls);
    }

    [Fact]
    public async Task StartAsync_without_a_prior_check_runs_CheckAsync_first()
    {
        var (update, _, store, _) = Harness.Create();
        store.Info = Harness.StoreAvailable("1.1.0");

        var started = await update.StartAsync();
        Assert.True(started.Started);
        Assert.NotNull(update.LastCheck);
        Assert.Equal(1, store.StartCalls);
    }

    [Fact]
    public async Task ClearPostponementAsync_makes_the_update_available_again()
    {
        var (update, _, store, _) = Harness.Create();
        store.Info = Harness.StoreAvailable("1.8.0");

        await update.CheckAsync();
        await update.PostponeAsync();
        await update.ClearPostponementAsync();

        var check = await update.CheckAsync();
        Assert.True(check.IsAvailable);
        Assert.False(check.IsPostponed);
    }

    [Fact]
    public async Task Downloaded_flexible_update_completes_instead_of_restarting()
    {
        var (update, _, store, _) = Harness.Create();
        store.Info = new StoreUpdateInfo
        {
            IsAvailable = true,
            LatestVersion = "1.2.0",
            FlexibleAllowed = true,
            InstallStatus = AppUpdateInstallStatus.Downloaded
        };

        var check = await update.CheckAsync();
        await update.StartAsync(check);

        Assert.Equal(1, store.CompleteCalls);
        Assert.Equal(0, store.StartCalls);
    }

    [Fact]
    public async Task Release_notes_and_minimum_version_are_surfaced_on_the_check()
    {
        var (update, _, store, _) = Harness.Create(options =>
        {
            options.PolicyProvider = new StaticAppUpdatePolicyProvider(Harness.Policy(p =>
            {
                p.LatestVersion = "3.0.0";
                p.MinimumSupportedVersion = "2.0.0";
                p.ReleaseNotes = "New onboarding and a login fix.";
                p.Priority = "mandatory";
            }));
        });
        store.Info = StoreUpdateInfo.None;

        var check = await update.CheckAsync();
        Assert.Equal("1.0.0", check.CurrentVersion);
        Assert.Equal("3.0.0", check.LatestVersion);
        Assert.Equal("2.0.0", check.MinimumSupportedVersion);
        Assert.Equal("New onboarding and a login fix.", check.ReleaseNotes);
        Assert.True(check.IsMandatory);
    }
}
