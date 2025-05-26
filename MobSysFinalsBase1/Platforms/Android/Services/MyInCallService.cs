using Android.App;
using Android.Content;
using Android.OS;
using Android.Telecom;
using Android.Media;
using Android.Runtime;
using Android.Net;

namespace MobSysFinalsBase1.Platforms.Android.Services
{
    [Service(Name = "com.companyname.mobsysfinalsbase1.MyInCallService",
             Permission = global::Android.Manifest.Permission.BindIncallService,
             Exported = true)]
    public class MyInCallService : InCallService
    {
        private Call _currentCallInstance;
        private static MyInCallService _staticInstance;

        public static MyInCallService Instance => _staticInstance;
        public static event EventHandler<CallState> CallStateChanged;

        public override void OnCreate()
        {
            base.OnCreate();
            _staticInstance = this;
        }

        public override void OnCallAdded(Call incomingCallObject)
        {
            base.OnCallAdded(incomingCallObject);
            if (incomingCallObject == null) return;

            _currentCallInstance = incomingCallObject;
            _currentCallInstance.RegisterCallback(new MyCallCallback(this));

            var intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);

            string phoneNumber = "Unknown";
            try
            {
                var details = _currentCallInstance.GetDetails();
                if (details != null)
                {
                    IntPtr handlePtr = details.Handle;
                    if (handlePtr != IntPtr.Zero)
                    {
                        var uri = global::Java.Lang.Object
                            .GetObject<global::Android.Net.Uri>(handlePtr, JniHandleOwnership.DoNotTransfer);
                        phoneNumber = uri?.SchemeSpecificPart ?? "Unknown";
                    }
                }
            }
            catch (global::Java.Lang.NoSuchMethodError)
            {
                phoneNumber = "Unknown";
            }
            catch (global::System.Exception)
            {
                phoneNumber = "Unknown";
            }

            intent.PutExtra("NavigateToCallScreen", true);
            intent.PutExtra("PhoneNumber", phoneNumber);
            StartActivity(intent);
        }

        public override void OnCallRemoved(Call removedCallObject)
        {
            base.OnCallRemoved(removedCallObject);
            if (_currentCallInstance == removedCallObject)
            {
                _currentCallInstance.UnregisterCallback(new MyCallCallback(this));
                _currentCallInstance = null;
            }

            CallStateChanged?.Invoke(null, CallState.Disconnected);

            var intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
            intent.PutExtra("CallEnded", true);
            StartActivity(intent);

            _staticInstance = null;
            StopSelf();
        }

        public void DisconnectCall()
        {
            _currentCallInstance?.Disconnect();
        }

        public void MuteCall(bool shouldMute)
        {
            var audioManager = (AudioManager)GetSystemService(Context.AudioService);
            if (audioManager != null)
            {
                audioManager.Mode = Mode.InCommunication;
                audioManager.MicrophoneMute = shouldMute;
            }
        }

        public void ToggleHold(bool shouldHold)
        {
            if (_currentCallInstance != null)
            {
                if (shouldHold)
                    _currentCallInstance.Hold();
                else
                    _currentCallInstance.Unhold();
            }
        }

        public void ToggleSpeakerphone(bool shouldBeOn)
        {
            var audioManager = (AudioManager)GetSystemService(Context.AudioService);
            if (audioManager != null)
            {
                audioManager.Mode = Mode.InCommunication;
                audioManager.SpeakerphoneOn = shouldBeOn;
            }
        }

        public class MyCallCallback : Call.Callback
        {
            private readonly MyInCallService _service;
            public MyCallCallback(MyInCallService service) => _service = service;

            public override void OnStateChanged(Call call, CallState state)
            {
                base.OnStateChanged(call, state);
                CallStateChanged?.Invoke(null, state);
            }
        }

        public override IBinder OnBind(Intent intent)
            => base.OnBind(intent);
    }
}
