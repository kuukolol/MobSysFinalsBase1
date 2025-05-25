using Microsoft.Extensions.Logging;
using MobSysFinalsBase1.Services;
using MobSysFinalsBase1.Shared;


#if ANDROID
using MobSysFinalsBase1.Platforms.Android.Services;
using MobSysFinalsBase1.Platforms.Android;
#endif

namespace MobSysFinalsBase1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.Services.AddBlazorBootstrap();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<DatabaseContext>();
            // Register the platform-specific dialer service for Android
#if ANDROID
            builder.Services.AddSingleton<IDialerPlatform, DialerPlatform>();
#else
            // Fallback for non-Android platforms
            builder.Services.AddSingleton<IDialerPlatform>(sp => new DefaultDialerPlatform());
#endif
            return builder.Build();
        }
    }

    // Default implementation for non-Android platforms (placeholder)
    public class DefaultDialerPlatform : IDialerPlatform
    {
        public void PlaceCall(string phoneNumber)
        {
            Console.WriteLine($"Dialing not supported on this platform for {phoneNumber}.");
        }
    }
}
