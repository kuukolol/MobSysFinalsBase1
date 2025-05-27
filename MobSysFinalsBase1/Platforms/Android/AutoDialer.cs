using Android.Content;
using Android;
using AndroidX.Core.Content;

namespace MyContact.Platforms
{
    public static class AutoDialer
    {
        public static void Call(string phoneNumber)
        {
            var context = Android.App.Application.Context;
            var intent = new Intent(Intent.ActionCall, Android.Net.Uri.Parse($"tel:{phoneNumber}"));
            intent.AddFlags(ActivityFlags.NewTask);
            if (ContextCompat.CheckSelfPermission(context, Manifest.Permission.CallPhone) != Android.Content.PM.Permission.Granted)
            {
                return;
            }
            context.StartActivity(intent);
        }
    }
}