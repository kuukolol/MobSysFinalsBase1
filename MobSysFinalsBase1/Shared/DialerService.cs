using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace MyContact.Shared
{
    public static class DialerService
    {
        public static async Task CallAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return;
            await Launcher.OpenAsync($"tel:{phoneNumber}");
        }

        public static void CallDirect(string phoneNumber)
        {
#if ANDROID
            MyContact.Platforms.AutoDialer.Call(phoneNumber);
#endif
        }

        public static string GetFullInternationalNumber(string phoneNumber, string countryCode)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return "";
            if (phoneNumber.StartsWith("+"))
                return phoneNumber;
            if (phoneNumber.StartsWith("0"))
                phoneNumber = phoneNumber.TrimStart('0');
            if (countryCode.StartsWith("+"))
                countryCode = countryCode.Substring(1);
            return $"+{countryCode}{phoneNumber}";
        }
    }
}