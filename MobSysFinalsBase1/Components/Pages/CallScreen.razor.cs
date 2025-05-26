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
using Android.Telecom;
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
#if ANDROID
            MyInCallService.CallStateChanged += OnCallStateChanged;
#endif
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
                DialerPlatform.PlaceCall(FormattedPhoneNumber);
            }
            catch
            {
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
                        _isCallConnected = false;
                        _ringingTimeoutTimer.Stop();
                        Nav.NavigateTo("/");
                    });
                }
            }
            else
            {
                _ringingTimeoutTimer.Stop();
            }
        }
        
        #if ANDROID


private void OnCallStateChanged(object sender, CallState state)
    {
        InvokeAsync(() =>
        {
            switch (state)
            {
                case CallState.Active:
                    _isCallConnected = true;
                    _isCallAnswered = true;
                    break;
                case CallState.Disconnected:
                    _isCallConnected = false;
                    break;
                case CallState.Ringing:
                    _isCallConnected = true;
                    break;
                case CallState.Dialing:
                    _isCallConnected = true;
                    break;
            }
            StateHasChanged();
        });
    }
#endif


    private void ToggleMute()
        {
#if ANDROID
            MyInCallService.Instance?.MuteCall(!IsMuted);
            IsMuted = !IsMuted;
            StateHasChanged();
#endif
        }

        private void ToggleSpeaker()
        {
#if ANDROID
            IsSpeakerOn = !IsSpeakerOn;
            MyInCallService.Instance?.ToggleSpeakerphone(IsSpeakerOn);
            StateHasChanged();
#endif
        }

        private async Task EndCall()
        {
            _callDurationTimer?.Stop();
            _ringingTimeoutTimer?.Stop();
#if ANDROID
            MyInCallService.Instance?.DisconnectCall();
#endif
            _isCallConnected = false;
            _isCallAnswered = false;
            Nav.NavigateTo("/");
        }

        public async void Dispose()
        {
            _callDurationTimer?.Stop();
            _ringingTimeoutTimer?.Stop();
#if ANDROID
            MyInCallService.CallStateChanged -= OnCallStateChanged;
#endif
        }
    }
}
