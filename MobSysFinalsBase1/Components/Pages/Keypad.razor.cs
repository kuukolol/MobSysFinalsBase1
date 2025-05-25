using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MobSysFinalsBase1.Components.Pages
{
    public partial class Keypad : ComponentBase
    {
        [Inject]
        public NavigationManager Nav { get; set; }

        protected string DialedNumber { get; set; } = string.Empty;
        protected List<string> KeypadKeys { get; set; } = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "*", "0", "#" };

        private void GoBack()
        {
            Nav.NavigateTo("/");
        }

        private void DialKey(string key)
        {
            DialedNumber += key;
            StateHasChanged();
        }

        private void ClearNumber()
        {
            if (!string.IsNullOrEmpty(DialedNumber))
            {
                DialedNumber = DialedNumber.Substring(0, DialedNumber.Length - 1);
                StateHasChanged();
            }
        }

        private void InitiateCall()
        {
            if (!string.IsNullOrEmpty(DialedNumber))
            {
                Nav.NavigateTo($"/call-screen/{DialedNumber}/unknown");
            }
        }
    }
}
