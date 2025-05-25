using Android.Content;
using Android.OS;
using Android.Telecom;
using MobSysFinalsBase1.Shared;

namespace MobSysFinalsBase1.Platforms.Android
{
    public class DialerPlatform : IDialerPlatform
    {
        public void PlaceCall(string phoneNumber)
        {
            var context = Platform.AppContext;
            var telecomManager = (TelecomManager)context.GetSystemService(Context.TelecomService);
            if (telecomManager != null)
            {
                var uri = global::Android.Net.Uri.Parse($"tel:{phoneNumber}");
                var extras = new Bundle();
                telecomManager.PlaceCall(uri, extras);
            }
        }
    }
}
