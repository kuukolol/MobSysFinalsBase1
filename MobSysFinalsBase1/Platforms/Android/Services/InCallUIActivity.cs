using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;

namespace MyContact.Platforms.Android.Services
{
    [Activity(Label = "InCallUIActivity",
              Theme = "@style/Maui.SplashTheme",
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
              LaunchMode = LaunchMode.SingleTask,
              Exported = true)]
    public class InCallUIActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView((Resource.Layout.incall_activity));
            bool callEnded = Intent.GetBooleanExtra("CallEnded", false);

            if (callEnded)
            {
                Finish();
                return;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        public override void OnBackPressed()
        {
        }
    }
}
