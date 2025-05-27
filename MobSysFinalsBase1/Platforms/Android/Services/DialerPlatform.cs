using Android.Content;
using Android.OS;
using Android.Telecom;
using Android;
using Android.Content.PM;
using Microsoft.Maui.ApplicationModel;
using AndroidX.Core.Content;
using MyContact.Shared;

namespace MyContact.Platforms.Android
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
                var componentName = new ComponentName(context, Java.Lang.Class.FromType(typeof(Services.MyConnectionService)).Name);
                var phoneAccountHandle = new PhoneAccountHandle(componentName, "MyDialer");
                extras.PutParcelable(TelecomManager.ExtraPhoneAccountHandle, phoneAccountHandle);

                if (context.CheckSelfPermission(Manifest.Permission.CallPhone) == PermissionChecker.PermissionGranted)
                {
                    telecomManager.PlaceCall(uri, extras);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("CALL_PHONE permission not granted.");
                }
            }
        }
    }
}
