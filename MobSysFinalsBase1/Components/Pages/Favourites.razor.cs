using Microsoft.AspNetCore.Components;
using MyContact.Models;
using MyContact.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MyContact.Components.Pages
{
    public partial class Favourites : ComponentBase
    {
        [Inject]
        public NavigationManager Nav { get; set; }

        [Inject]
        public DatabaseContext DB { get; set; }

        private List<User> favouriteContacts = new();
        private string searchText = "";
        private string selectedLetter = "";
        protected bool ShowToast { get; set; } = false;
        protected string ToastMessage { get; set; } = "";

        private IEnumerable<IGrouping<string, User>> groupedFavouriteContacts =>
            favouriteContacts
                .Where(c => string.IsNullOrWhiteSpace(searchText) ||
                            DisplayName(c).Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .Where(c => string.IsNullOrWhiteSpace(selectedLetter) ||
                            DisplayName(c).StartsWith(selectedLetter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => DisplayName(c))
                .GroupBy(c => DisplayName(c).Substring(0, 1).ToUpper());

        protected override async Task OnInitializedAsync()
        {
            Debug.WriteLine("[Favourites] OnInitializedAsync started.");
            if (DB != null)
            {
                await DB.Init();
                Debug.WriteLine("[Favourites] Database initialized.");
                await LoadFavouriteContacts();
            }
        }

        private async Task LoadFavouriteContacts()
        {
            if (DB != null)
            {
                var allContacts = await DB.Users();
                favouriteContacts = allContacts.Where(c => c.Favourites).ToList();
                StateHasChanged();
                Debug.WriteLine("[Favourites] Favourite contacts loaded and StateHasChanged called.");
            }
        }

        private void NavigateToContactDetail(int contactId)
        {
            Nav.NavigateTo($"/contact/{contactId}");
            Debug.WriteLine($"[Favourites] Navigated to contact detail: ID={contactId}");
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

        private string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                Debug.WriteLine($"[Favourites] Search text updated to: {searchText}");
                StateHasChanged();
            }
        }

        private string GetInitial(User user)
        {
            var name = DisplayName(user);
            return !string.IsNullOrEmpty(name)
                ? name.Substring(0, 1).ToUpper()
                : "?";
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

        private string GetStarIconClass(User user)
        {
            return user.Favourites ? "fas fa-star" : "far fa-star";
        }

        private string GetStarColor(User user)
        {
            return user.Favourites ? "#facc15" : "#374151";
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
    }
}
