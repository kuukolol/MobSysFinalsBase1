using Microsoft.AspNetCore.Components;
using MyContact.Shared;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace MyContact.Components.Pages
{
    public partial class CallScreen : ComponentBase, IDisposable
    {
        [Parameter] public string PhoneNumber { get; set; }
        [Parameter] public string CountryCode { get; set; }
        [Parameter] public string ContactName { get; set; }

        [Inject] public NavigationManager Nav { get; set; }

        protected bool IsMuted { get; set; }
        protected bool IsSpeakerOn { get; set; }
        protected string CallDuration { get; set; } = "00:00";
        protected string FormattedPhoneNumber { get; set; }

        private int _secondsElapsed;
        private System.Timers.Timer _callDurationTimer;

        protected override async Task OnInitializedAsync()
        {
            _callDurationTimer = new System.Timers.Timer(1000);
            _callDurationTimer.Elapsed += UpdateCallDuration;
            _callDurationTimer.Start();
            FormatPhoneNumber();
#if ANDROID
            DialerService.CallDirect(FormattedPhoneNumber);
#endif
        }

        private void FormatPhoneNumber()
        {
            FormattedPhoneNumber = DialerService.GetFullInternationalNumber(PhoneNumber, CountryCode);
        }

        private async Task InitiateCall()
        {
            try
            {
                await DialerService.CallAsync(FormattedPhoneNumber);
            }
            catch
            {
                Nav.NavigateTo("/");
            }
        }

        private void UpdateCallDuration(object sender, ElapsedEventArgs e)
        {
            _secondsElapsed++;
            int m = _secondsElapsed / 60;
            int s = _secondsElapsed % 60;
            CallDuration = $"{m:D2}:{s:D2}";
            InvokeAsync(StateHasChanged);
        }

        private async Task EndCall()
        {
            _callDurationTimer?.Stop();
            Nav.NavigateTo("/");
        }

        private void ToggleMute() { }
        private void ToggleSpeaker() { }

        public void Dispose()
        {
            _callDurationTimer?.Stop();
        }
    }
}