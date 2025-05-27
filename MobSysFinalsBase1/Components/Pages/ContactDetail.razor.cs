using Microsoft.AspNetCore.Components;
using MyContact.Models;
using MyContact.Shared;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyContact.Components.Pages
{
    public partial class ContactDetail : ComponentBase
    {
        [Parameter]
        public int ContactId { get; set; }

        [Inject]
        public NavigationManager Nav { get; set; }

        [Inject]
        public DatabaseContext DB { get; set; }

        protected User Contact { get; set; }
        protected bool IsFavorite { get; set; }
        protected bool ShowToast { get; set; }
        protected string ToastMessage { get; set; }

        protected string StarIconClass => IsFavorite ? "fas fa-star" : "far fa-star";
        protected string StarColorClass => IsFavorite ? "text-yellow-300" : "text-black-800";

        protected override async Task OnInitializedAsync()
        {
            if (DB != null)
            {
                await DB.Init();
                await LoadContact();
            }
        }

        private async Task LoadContact()
        {
            var users = await DB.Users();
            Contact = users.FirstOrDefault(u => u.ID == ContactId);
            if (Contact != null)
            {
                IsFavorite = Contact.Favourites;
            }
            StateHasChanged();
        }

        private void GoBack()
        {
            Nav.NavigateTo("/");
        }

        private void EditContact()
        {
            Nav.NavigateTo($"/edit-contact/{ContactId}");
        }

        private async Task InitiateCall()
        {
            if (Contact == null || string.IsNullOrWhiteSpace(Contact.PhoneNumber))
            {
                await ShowToastMessage("No phone number available to call.");
                return;
            }
            var fullNumber = DialerService.GetFullInternationalNumber(Contact.PhoneNumber, Contact.CountryCode);

            await ShowToastMessage($"Dialing: {fullNumber}");

            var status = await Permissions.RequestAsync<Permissions.Phone>();
            if (status != PermissionStatus.Granted)
            {
                await ShowToastMessage("Phone permission not granted.");
                return;
            }

            DialerService.CallDirect(fullNumber);
        }

        private async Task ToggleFavorite()
        {
            if (Contact == null) return;
            IsFavorite = !IsFavorite;
            Contact.Favourites = IsFavorite;
            await DB.SaveUser(Contact);
            await ShowToastMessage(IsFavorite ? "Added to Favourites! 🎉" : "Removed from Favourites.");
        }

        private async Task ShowToastMessage(string message)
        {
            ToastMessage = message;
            ShowToast = true;
            StateHasChanged();
            await Task.Delay(1800);
            ShowToast = false;
            StateHasChanged();
        }

        private string GetInitial(User user)
        {
            var name = DisplayName(user);
            return !string.IsNullOrEmpty(name) ? name.Substring(0, 1).ToUpper() : "?";
        }

        private string DisplayName(User user)
        {
            if (user == null) return "Unknown";
            if (!string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.Surname))
                return $"{user.FirstName} {user.Surname}";
            if (!string.IsNullOrWhiteSpace(user.FirstName))
                return user.FirstName;
            if (!string.IsNullOrWhiteSpace(user.Surname))
                return user.Surname;
            return user.PhoneNumber ?? "Unknown";
        }

        private string GetImageSrc(string filePath)
        {
            if (filePath.StartsWith("data:") || filePath.StartsWith("images/"))
                return filePath;
            if (File.Exists(filePath))
            {
                var bytes = File.ReadAllBytes(filePath);
                var base64 = Convert.ToBase64String(bytes);
                var ext = Path.GetExtension(filePath).ToLower();
                var mime = ext == ".svg" ? "image/svg+xml" : ext == ".png" ? "image/png" : "image/jpeg";
                return $"data:{mime};base64,{base64}";
            }
            var rnd = new Random();
            return $"images/default_avatar_{rnd.Next(1, 5)}.jpg";
        }
    }
}