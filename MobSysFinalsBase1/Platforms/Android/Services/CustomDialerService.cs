using Android.Content;
using Android.Net;           
using Android.OS;
using Android.Telecom;
using Microsoft.Maui.ApplicationModel;

namespace MyContact.Platforms.Android.Services
{
    public class CustomDialerService
    {
        public static void PlaceCall(string phoneNumber)
        {
            var context = Platform.AppContext;
            var telecomManager = context.GetSystemService(Context.TelecomService) as TelecomManager;
            if (telecomManager != null)
            {
                // Use fully qualified name to avoid ambiguity
                var uri = global::Android.Net.Uri.Parse($"tel:{phoneNumber}");
                var extras = new Bundle();
                extras.PutBoolean(TelecomManager.ExtraStartCallWithSpeakerphone, true);
                telecomManager.PlaceCall(uri, extras);
            }
        }
    }
}
