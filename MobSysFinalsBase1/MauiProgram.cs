using Microsoft.Extensions.Logging;
using MyContact.Services; 
using MyContact.Shared;  

#if ANDROID

using MyContact.Platforms.Android; // For DialerPlatform

#endif

namespace MyContact
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

#if ANDROID
            builder.Services.AddSingleton<IDialerPlatform, DialerPlatform>();
#else
            builder.Services.AddSingleton<IDialerPlatform>(sp => new DefaultDialerPlatform());
#endif
#if ANDROID
    builder.Services.AddSingleton<IDefaultDialerService, MyContact.Platforms.Android.DefaultDialerService>();
#endif
            return builder.Build();
        }
    }

    public class DefaultDialerPlatform : IDialerPlatform
    {
        public void PlaceCall(string phoneNumber)
        {
            System.Diagnostics.Debug.WriteLine($"Dialing not supported on this platform for {phoneNumber}.");
        }
    }
}
