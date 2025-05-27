using Android.Content;
using Android.Telecom;
using Android.App;
using MyContact.Services;
using System.Diagnostics;
namespace MyContact.Platforms.Android
{
    public class DefaultDialerService : IDefaultDialerService
    {
        public Task<bool> RequestDefaultDialerAsync()
        {
            try
            {
                var context = Platform.CurrentActivity ?? Platform.AppContext;
                var telecomManager = (TelecomManager)context.GetSystemService(Context.TelecomService);
               
                if (telecomManager.DefaultDialerPackage == context.PackageName)
                {
                    return Task.FromResult(true);
                }
                var intent = new Intent(TelecomManager.ActionChangeDefaultDialer);
                intent.PutExtra(TelecomManager.ExtraChangeDefaultDialerPackageName, context.PackageName);
                intent.AddFlags(ActivityFlags.NewTask);
                context.StartActivity(intent);
                 Debug.WriteLine(context.PackageName);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DefaultDialerService error: {ex}");
                return Task.FromResult(false);
            }
        }
    }
}