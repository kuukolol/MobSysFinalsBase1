using Microsoft.AspNetCore.Components;
using MyContact.Shared;
using MyContact.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MyContact.Components.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject]
        public NavigationManager Nav { get; set; }

        [Inject]
        public DatabaseContext DB { get; set; }

        private List<User> contacts = new();
        private string searchText = "";
        private string selectedLetter = "";
        protected bool ShowToast { get; set; } = false;
        protected string ToastMessage { get; set; } = "";

        private IEnumerable<IGrouping<string, User>> groupedContacts =>
            contacts
                .Where(c => string.IsNullOrWhiteSpace(searchText) ||
                            contactDisplayName(c).Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .Where(c => string.IsNullOrWhiteSpace(selectedLetter) ||
                            contactDisplayName(c).StartsWith(selectedLetter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => contactDisplayName(c))
                .GroupBy(c => contactDisplayName(c).Substring(0, 1).ToUpper());

        protected override async Task OnInitializedAsync()
        {
            Debug.WriteLine("[Home] OnInitializedAsync started.");
            if (DB != null)
            {
                await DB.Init();
                Debug.WriteLine("[Home] Database initialized.");
            }
            await LoadContacts();
            var uri = new Uri(Nav.Uri);
            Debug.WriteLine($"[Home] Current URI: {uri}");
            if (uri.Query.Contains("success=true&mode=create"))
            {
                Debug.WriteLine("[Home] success=true query parameter detected.");
                ShowToastMessage("Contact created successfully! 🎉");
                Nav.NavigateTo("/", false);
                Debug.WriteLine("[Home] Navigated to '/' to clear query parameter.");
            }
            else if (uri.Query.Contains("success=true&mode=edit"))
            {
                Debug.WriteLine("[Home] success=true query parameter detected.");
                ShowToastMessage("Contact updated successfully! 🎉");
                Nav.NavigateTo("/", false);
                Debug.WriteLine("[Home] Navigated to '/' to clear query parameter.");
            }
            else
            {
                Debug.WriteLine("[Home] No success=true query parameter found in URI.");
            }
        }

        private async Task LoadContacts()
        {
            if (DB != null)
            {
                contacts = await DB.Users();
                StateHasChanged();
                Debug.WriteLine("[Home] Contacts loaded and StateHasChanged called.");
            }
        }

        private void NavigateToAddContact()
        {
            Nav.NavigateTo("/add-contact");
            Debug.WriteLine("[Home] Navigated to /add-contact.");
        }

        private async void ShowToastMessage(string message)
        {
            try
            {
                ToastMessage = message;
                ShowToast = true;
                Debug.WriteLine($"[Home] Toast shown with message: {message}");
                StateHasChanged();
                Debug.WriteLine("[Home] StateHasChanged called after showing toast.");
                await Task.Delay(1800);
                ShowToast = false;
                StateHasChanged();
                Debug.WriteLine("[Home] Toast auto-hidden after delay.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Home] Failed to show toast: {ex.Message}");
            }
        }

        private string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                Debug.WriteLine($"[Home] Search text updated to: {searchText}");
                StateHasChanged();
            }
        }

        private void FilterByLetter(string letter)
        {
            if (selectedLetter == letter)
            {
                selectedLetter = "";
            }
            else
            {
                selectedLetter = letter;
            }
            StateHasChanged();
        }

        private string GetInitial(User user)
        {
            var name = contactDisplayName(user);
            return !string.IsNullOrEmpty(name)
                ? name.Substring(0, 1).ToUpper()
                : "?";
        }

        private string contactDisplayName(User user)
        {
            if (!string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.Surname))
                return $"{user.FirstName} {user.Surname}";
            if (!string.IsNullOrWhiteSpace(user.FirstName))
                return user.FirstName;
            if (!string.IsNullOrWhiteSpace(user.Surname))
                return user.Surname;
            return user.PhoneNumber ?? "";
        }

        private string GetImageSrc(string filePath)
        {
            if (filePath.StartsWith("data:image/"))
                return filePath;
            if (filePath.StartsWith("images/"))
                return filePath;
            if (File.Exists(filePath))
            {
                try
                {
                    byte[] fileBytes = File.ReadAllBytes(filePath);
                    string base64String = Convert.ToBase64String(fileBytes);
                    string extension = Path.GetExtension(filePath).ToLower();
                    string mimeType = extension == ".svg" ? "image/svg+xml" : (extension == ".png" ? "image/png" : "image/jpeg");
                    return $"data:{mimeType};base64,{base64String}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting file to base64: {ex.Message}");
                    Random rand = new Random();
                    int avatarNum = rand.Next(1, 5);
                    return $"images/default_avatar_{avatarNum}.jpg";
                }
            }
            Random randFallback = new Random();
            int fallbackAvatarNum = randFallback.Next(1, 5);
            return $"images/default_avatar_{fallbackAvatarNum}.jpg";
        }

        private void NavigateToContactDetail(int contactId)
        {
            Nav.NavigateTo($"/contact/{contactId}");
            Debug.WriteLine($"[Home] Navigated to contact detail: ID={contactId}");
        }

    }
}
