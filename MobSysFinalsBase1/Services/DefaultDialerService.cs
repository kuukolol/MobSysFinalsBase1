using MyContact.Services;

public static class DefaultDialerService
{
    public static Task<bool> RequestDefaultDialerAsync()
    {
#if ANDROID
        var svc = Microsoft.Maui.Controls.DependencyService.Get<IDefaultDialerService>();
        return svc?.RequestDefaultDialerAsync() ?? Task.FromResult(false);
#else
        return Task.FromResult(false);
#endif
    }
}