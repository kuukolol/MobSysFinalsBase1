using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Telecom;
using Android.Widget;
using Android.Runtime;
using Android.Util;
using Java.Lang;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Threading.Tasks;
using AndroidUri = Android.Net.Uri;

namespace MyContact
{
    [Activity(Theme = "@style/Maui.SplashTheme",
              MainLauncher = true,
              ConfigurationChanges = ConfigChanges.ScreenSize
                                  | ConfigChanges.Orientation
                                  | ConfigChanges.UiMode
                                  | ConfigChanges.ScreenLayout
                                  | ConfigChanges.SmallestScreenSize
                                  | ConfigChanges.Density,
              LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : MauiAppCompatActivity
    {
        const int REQUEST_OVERLAY_PERMISSION = 1234;
        const int REQUEST_DIALER_ROLE = 5678;
        const int REQUEST_CALL_PHONE_PERMISSION = 5679;
        const int REQUEST_CODE_SET_DEFAULT_DIALER = 4321;
        const int CAPABILITY_CALL_PROVIDER = 0x00000002; 
        const int CAPABILITY_SELF_MANAGED = 0x00001000; 

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ProcessIntent(Intent);
            RequestEssentialPermissionsAndRoles();
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            ProcessIntent(intent);
        }

        void ProcessIntent(Intent intent)
        {
            if (intent?.GetBooleanExtra("NavigateToCallScreen", false) == true)
            {
                string phoneNumber = intent.GetStringExtra("PhoneNumber") ?? "Unknown";
                string contactName = intent.GetStringExtra("ContactName") ?? "Unknown";
                MainThread.BeginInvokeOnMainThread(async () =>
                    await Shell.Current.GoToAsync(
                        $"//call-screen/{System.Net.WebUtility.UrlEncode(phoneNumber)}/{System.Net.WebUtility.UrlEncode(contactName)}",
                        true
                    )
                );
            }
            else if (intent?.GetBooleanExtra("CallEnded", false) == true)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (Shell.Current.CurrentState?.Location?.OriginalString?.Contains("call-screen") == true)
                        await Shell.Current.GoToAsync("..", true);
                });
            }
        }

        void RequestEssentialPermissionsAndRoles()
        {
            if (!Settings.CanDrawOverlays(this))
            {
                var overlayIntent = new Intent(
                    Settings.ActionManageOverlayPermission,
                    AndroidUri.Parse($"package:{PackageName}")
                );
                StartActivityForResult(overlayIntent, REQUEST_OVERLAY_PERMISSION);
                return;
            }

            if (CheckSelfPermission(Manifest.Permission.CallPhone) != Permission.Granted)
            {
                RequestPermissions(
                    new[] { Manifest.Permission.CallPhone },
                    REQUEST_CALL_PHONE_PERMISSION
                );
                return;
            }

            _ = AttemptToSetDefaultDialer();
        }

        async Task AttemptToSetDefaultDialer()
        {
            var tm = (TelecomManager)GetSystemService(Context.TelecomService);
            if (tm == null) return;

            if (!PackageName.Equals(tm.DefaultDialerPackage))
            {
                var intent = new Intent(TelecomManager.ActionChangeDefaultDialer);
                intent.PutExtra(TelecomManager.ExtraChangeDefaultDialerPackageName, PackageName);
                StartActivityForResult(intent, REQUEST_CODE_SET_DEFAULT_DIALER);
                Toast.MakeText(this, "enabled", ToastLength.Short);
            }
            else
            {
                RegisterPhoneAccountIfDefaultDialer();
                Toast.MakeText(this, "not enabled", ToastLength.Short);

            }
        }

        public override void OnRequestPermissionsResult(
            int requestCode,
            string[] permissions,
            [GeneratedEnum] Permission[] grantResults
        )
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == REQUEST_CALL_PHONE_PERMISSION &&
                grantResults.Length > 0 &&
                grantResults[0] == Permission.Granted)
            {
                _ = AttemptToSetDefaultDialer();
            }
            else if (requestCode == REQUEST_CALL_PHONE_PERMISSION)
            {
                Toast.MakeText(
                    this,
                    "Call Phone permission is required for this app to function as a dialer.",
                    ToastLength.Long
                ).Show();
            }
        }

        protected override void OnActivityResult(
            int requestCode,
            [GeneratedEnum] Result resultCode,
            Intent data
        )
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == REQUEST_OVERLAY_PERMISSION)
            {
                if (Settings.CanDrawOverlays(this))
                    RequestEssentialPermissionsAndRoles();
                else
                    Toast.MakeText(
                        this,
                        "Overlay permission is needed for call UI.",
                        ToastLength.Long
                    ).Show();
            }
            else if (requestCode == REQUEST_DIALER_ROLE)
            {
                var tm = (TelecomManager)GetSystemService(Context.TelecomService);
                if (tm != null && PackageName.Equals(tm.DefaultDialerPackage))
                {
                    Toast.MakeText(this, "Dialer role granted.", ToastLength.Long).Show();
                    RegisterPhoneAccountIfDefaultDialer();
                }
                else
                {
                    Toast.MakeText(this, tm.DefaultDialerPackage, ToastLength.Long).Show();
                    RegisterPhoneAccountIfDefaultDialer();
                }
            }
        }

        void RegisterPhoneAccountIfDefaultDialer()
        {
            var tm = (TelecomManager)GetSystemService(Context.TelecomService);
            if (tm == null ||
                !PackageName.Equals(tm.DefaultDialerPackage) ||
                CheckSelfPermission(Manifest.Permission.CallPhone) != Permission.Granted)
                return;

            var component = new ComponentName(this, Java.Lang.Class.FromType(typeof(MyContact.Platforms.Android.Services.MyConnectionService)));
            var handle = new PhoneAccountHandle(component, "MyDialer");
            var builder = new PhoneAccount.Builder(handle, "My Dialer App");
            builder.SetCapabilities(CAPABILITY_CALL_PROVIDER);
            builder.AddSupportedUriScheme(PhoneAccount.SchemeTel);
            var account = builder.Build();

            try
            {
                tm.RegisterPhoneAccount(account);
            }
            catch (System.Exception ex)
            {
                Log.Error("MainActivity", ex.Message);
            }
        }
    }
}
