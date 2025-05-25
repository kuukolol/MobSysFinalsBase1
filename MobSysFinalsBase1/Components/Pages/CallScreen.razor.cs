using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MobSysFinalsBase1.Shared;
using System;
using System.Threading.Tasks;
using System.Timers;
#if ANDROID
using Android.App;
using Android.Content;
using Java.Lang;
using MobSysFinalsBase1.Platforms.Android.Services;
#endif

namespace MobSysFinalsBase1.Components.Pages
{
    public partial class CallScreen : ComponentBase, IDisposable
    {
        [Parameter] public string PhoneNumber { get; set; }
        [Parameter] public string ContactName { get; set; }

        [Inject] public NavigationManager Nav { get; set; }
        [Inject] public IJSRuntime JS { get; set; }
        [Inject] public IDialerPlatform DialerPlatform { get; set; }

        protected bool IsMuted { get; set; }
        protected bool IsSpeakerOn { get; set; }
        protected string CallDuration { get; set; } = "00:00";
        protected string FormattedPhoneNumber { get; set; }

        private int _secondsElapsed;
        private int _ringingSeconds;
        private System.Timers.Timer _callDurationTimer;
        private System.Timers.Timer _ringingTimeoutTimer;
        private bool _isCallConnected;
        private bool _isCallAnswered;

        protected override async Task OnInitializedAsync()
        {
            _callDurationTimer = new System.Timers.Timer(1000);
            _callDurationTimer.Elapsed += UpdateCallDuration;
            _callDurationTimer.Start();

            _ringingTimeoutTimer = new System.Timers.Timer(1000);
            _ringingTimeoutTimer.Elapsed += CheckRingingTimeout;
            _ringingTimeoutTimer.Start();

            FormatPhoneNumber();
            //await JS.InvokeVoidAsync("playRingingSound");
            await InitiateCall();
        }

        private void FormatPhoneNumber()
        {
            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                FormattedPhoneNumber = "Unknown Number";
            }
            else if (!PhoneNumber.StartsWith("+"))
            {
                FormattedPhoneNumber = $"+{PhoneNumber}";
            }
            else
            {
                FormattedPhoneNumber = PhoneNumber;
            }
        }

        private async Task InitiateCall()
        {
            try
            {
#if ANDROID
                var ctx = Android.App.Application.Context;
                if (Android.Provider.Settings.CanDrawOverlays(ctx))
                {
                    var overlayIntent = new Intent(ctx, Class.FromType(typeof(CallOverlayService)));
                    ctx.StartService(overlayIntent);
                }
#endif

                DialerPlatform.PlaceCall(FormattedPhoneNumber);
            }
            catch
            {
                //await JS.InvokeVoidAsync("stopRingingSound");
                Nav.NavigateTo("/");
            }
        }

        private void UpdateCallDuration(object sender, ElapsedEventArgs e)
        {
            if (_isCallConnected && _isCallAnswered)
            {
                _secondsElapsed++;
                int m = _secondsElapsed / 60;
                int s = _secondsElapsed % 60;
                CallDuration = $"{m:D2}:{s:D2}";
                InvokeAsync(StateHasChanged);
            }
        }

        private void CheckRingingTimeout(object sender, ElapsedEventArgs e)
        {
            if (!_isCallAnswered)
            {
                _ringingSeconds++;
                if (_ringingSeconds >= 30)
                {
                    InvokeAsync(async () =>
                    {
                        await JS.InvokeVoidAsync("stopRingingSound");
                        _isCallConnected = false;
                        _ringingTimeoutTimer.Stop();
                        StopOverlay();
                        Nav.NavigateTo("/");
                    });
                }
            }
            else
            {
                _ringingTimeoutTimer.Stop();
            }
        }

        private void ToggleMute()
        {
            IsMuted = !IsMuted;
            StateHasChanged();
        }

        private void ToggleSpeaker()
        {
            IsSpeakerOn = !IsSpeakerOn;
            StateHasChanged();
        }

        private async Task EndCall()
        {
            _callDurationTimer?.Stop();
            _ringingTimeoutTimer?.Stop();
            //await JS.InvokeVoidAsync("stopRingingSound");
            StopOverlay();
            _isCallConnected = false;
            _isCallAnswered = false;
            Nav.NavigateTo("/");
        }

        private void StopOverlay()
        {
#if ANDROID
            var ctx = Android.App.Application.Context;
            var stopIntent = new Intent(ctx, Class.FromType(typeof(CallOverlayService)));
            ctx.StopService(stopIntent);
#endif
        }

        public async void Dispose()
        {
            _callDurationTimer?.Stop();
            _ringingTimeoutTimer?.Stop();
            //await JS.InvokeVoidAsync("stopRingingSound");
            StopOverlay();
        }
    }
}
