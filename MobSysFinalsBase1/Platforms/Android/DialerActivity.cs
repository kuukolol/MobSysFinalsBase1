using Android.App;
using Android.Content.PM;
using Android.OS;

namespace MyContact
{
    [Activity(Label = "My Dialer", Exported = true, LaunchMode = LaunchMode.SingleTask)]
    public class DialerActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Android.Resource.Layout.SimpleListItem1);
        }
    }
}
