using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Graphics;
using View = Android.Views.View;

namespace MyContact.Platforms.Android.Services
{
    [Service]
    public class CallOverlayService : Service
    {
        IWindowManager windowManager;
        View overlayView;
        public override void OnCreate()
        {
            base.OnCreate();
            var layoutParams = new WindowManagerLayoutParams(WindowManagerLayoutParams.MatchParent, WindowManagerLayoutParams.MatchParent, WindowManagerTypes.ApplicationOverlay, WindowManagerFlags.NotFocusable | WindowManagerFlags.LayoutInScreen | WindowManagerFlags.NotTouchModal, Format.Translucent);
            overlayView = LayoutInflater.From(this).Inflate(Resource.Layout.call_overlay, null);
            windowManager = GetSystemService(WindowService) as IWindowManager;
            windowManager?.AddView(overlayView, layoutParams);
        }
        public override void OnDestroy()
        {
            if (overlayView != null)
            {
                windowManager?.RemoveView(overlayView);
                overlayView = null;
            }
            base.OnDestroy();
        }
        public override IBinder OnBind(Intent intent) => null;
    }
}
