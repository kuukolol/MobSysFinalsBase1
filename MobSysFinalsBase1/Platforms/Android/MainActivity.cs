using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Net;
using Android.Provider;
using Android.Telecom;
using Android.Widget;
using AndroidUri = Android.Net.Uri;
using MobSysFinalsBase1.Platforms.Android.Services;

namespace MobSysFinalsBase1
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        const int REQUEST_OVERLAY_PERMISSION = 1234;
        const int REQUEST_DIALER_ROLE = 5678;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            var telecomManager = (TelecomManager)GetSystemService(TelecomService);
            System.Diagnostics.Debug.WriteLine("Default Dialer Package: " + telecomManager.DefaultDialerPackage);

            base.OnCreate(savedInstanceState);
#if ANDROID
            if (!Settings.CanDrawOverlays(this))
            {
                var overlayIntent = new Intent(Settings.ActionManageOverlayPermission, AndroidUri.Parse($"package:{PackageName}"));
                StartActivityForResult(overlayIntent, REQUEST_OVERLAY_PERMISSION);
            }
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                var roleManagerObj = GetSystemService("role");
                if (roleManagerObj != null)
                {
                    dynamic roleManager = roleManagerObj;
                    string roleDialer = "android.app.role.DIALER";
                    bool isHeld = roleManager.IsRoleHeld(roleDialer);
                    if (!isHeld)
                    {
                        Intent roleRequestIntent = roleManager.CreateRequestRoleIntent(roleDialer);
                        StartActivityForResult(roleRequestIntent, REQUEST_DIALER_ROLE);
                    }
                }
            }
            else
            {
                var telecomManagerFallback = (TelecomManager)GetSystemService(TelecomService);
                if (telecomManagerFallback != null && !PackageName.Equals(telecomManagerFallback.DefaultDialerPackage))
                {
                    var dialerIntent = new Intent(TelecomManager.ActionChangeDefaultDialer);
                    dialerIntent.PutExtra(TelecomManager.ExtraChangeDefaultDialerPackageName, PackageName);
                    StartActivityForResult(dialerIntent, REQUEST_DIALER_ROLE);
                }
            }
            RegisterPhoneAccountIfDefaultDialer();
#endif
        }
#if ANDROID
        void RegisterPhoneAccountIfDefaultDialer()
        {
            var telecomManager = (TelecomManager)GetSystemService(TelecomService);
            if (telecomManager != null && PackageName.Equals(telecomManager.DefaultDialerPackage))
            {
                var componentName = new ComponentName(PackageName, typeof(MyConnectionService).FullName);
                var phoneAccountHandle = new PhoneAccountHandle(componentName, "MyDialer");
                var builder = new PhoneAccount.Builder(phoneAccountHandle, "My Dialer App");
                builder.SetCapabilities(1);
                PhoneAccount phoneAccount = builder.Build();
                telecomManager.RegisterPhoneAccount(phoneAccount);
            }
        }
#endif
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
#if ANDROID
            if (requestCode == REQUEST_OVERLAY_PERMISSION)
            {
                if (!Settings.CanDrawOverlays(this))
                {
                    Toast.MakeText(this, "Overlay permission not granted.", ToastLength.Long).Show();
                }
            }
            else if (requestCode == REQUEST_DIALER_ROLE)
            {
                var telecomManager = (TelecomManager)GetSystemService(TelecomService);
                if (telecomManager != null && PackageName.Equals(telecomManager.DefaultDialerPackage))
                {
                    RegisterPhoneAccountIfDefaultDialer();
                    Toast.MakeText(this, "Dialer role granted.", ToastLength.Long).Show();
                }
                else
                {
                    Toast.MakeText(this, "Dialer role not granted.", ToastLength.Long).Show();
                }
            }
#endif
        }
    }
}
