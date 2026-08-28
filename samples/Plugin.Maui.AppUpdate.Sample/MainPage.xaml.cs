using Plugin.Maui.AppUpdate;

namespace Plugin.Maui.AppUpdate.Sample;

public partial class MainPage : ContentPage
{
    readonly IAppUpdate _appUpdate;
    readonly DemoStoreClient _store;
    readonly DemoPolicyProvider _policy;

    public MainPage(IAppUpdate appUpdate, DemoStoreClient store, DemoPolicyProvider policy)
    {
        InitializeComponent();
        _appUpdate = appUpdate;
        _store = store;
        _policy = policy;
        _appUpdate.Checked += (_, result) => MainThread.BeginInvokeOnMainThread(() => Render(result));
        StatusLabel.Text = "Installed 1.0.0 · demo store + policy (no Play/App Store required)";
    }

    async void OnRecommendedClicked(object? sender, EventArgs e)
    {
        _policy.Policy = DemoPolicyProvider.Recommended();
        _store.Info = new StoreUpdateInfo
        {
            IsAvailable = true,
            LatestVersion = "1.2.0",
            FlexibleAllowed = true,
            StoreUrl = "https://example.com/store"
        };
        await CheckAsync();
    }

    async void OnMandatoryClicked(object? sender, EventArgs e)
    {
        _policy.Policy = DemoPolicyProvider.Mandatory();
        _store.Info = new StoreUpdateInfo
        {
            IsAvailable = true,
            LatestVersion = "2.0.0",
            ImmediateAllowed = true,
            FlexibleAllowed = true,
            Priority = 5,
            StoreUrl = "https://example.com/store"
        };
        await CheckAsync();
    }

    async void OnMinimumClicked(object? sender, EventArgs e)
    {
        _policy.Policy = DemoPolicyProvider.MinimumVersion();
        _store.Info = StoreUpdateInfo.None;
        await CheckAsync();
    }

    async void OnMaintenanceClicked(object? sender, EventArgs e)
    {
        _policy.Policy = DemoPolicyProvider.Maintenance();
        _store.Info = StoreUpdateInfo.None;
        await CheckAsync();
    }

    async void OnCheckClicked(object? sender, EventArgs e) => await CheckAsync();

    async void OnStartClicked(object? sender, EventArgs e)
    {
        try
        {
            var started = await _appUpdate.StartAsync();
            ResultLabel.Text += $"{Environment.NewLine}{Environment.NewLine}Start {started.Status} · {started.Flow}{Environment.NewLine}{started.Message}";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = ex.Message;
        }
    }

    async void OnPostponeClicked(object? sender, EventArgs e)
    {
        try
        {
            await _appUpdate.PostponeAsync(TimeSpan.FromDays(3));
            await CheckAsync();
        }
        catch (Exception ex)
        {
            ResultLabel.Text = ex.Message;
        }
    }

    async void OnClearPostponeClicked(object? sender, EventArgs e)
    {
        await _appUpdate.ClearPostponementAsync();
        await CheckAsync();
    }

    async Task CheckAsync()
    {
        try
        {
            Render(await _appUpdate.CheckAsync());
        }
        catch (Exception ex)
        {
            ResultLabel.Text = ex.Message;
        }
    }

    void Render(AppUpdateCheckResult result)
    {
        ResultLabel.Text =
            $"Available     {result.IsAvailable}{Environment.NewLine}" +
            $"Has update    {result.HasUpdate}{Environment.NewLine}" +
            $"Mandatory     {result.IsMandatory}{Environment.NewLine}" +
            $"Recommended   {result.IsRecommended}{Environment.NewLine}" +
            $"Maintenance   {result.IsMaintenance}{Environment.NewLine}" +
            $"Can postpone  {result.CanPostpone}{Environment.NewLine}" +
            $"Postponed     {result.IsPostponed}{Environment.NewLine}" +
            $"Priority      {result.Priority}{Environment.NewLine}" +
            $"Flow          {result.Flow}{Environment.NewLine}" +
            $"Reason        {result.Reason}{Environment.NewLine}" +
            $"Current       {result.CurrentVersion}{Environment.NewLine}" +
            $"Latest        {result.LatestVersion ?? "—"}{Environment.NewLine}" +
            $"Min supported {result.MinimumSupportedVersion ?? "—"}{Environment.NewLine}" +
            $"Notes         {result.ReleaseNotes ?? "—"}{Environment.NewLine}" +
            $"Message       {result.Message ?? "—"}";
    }
}
