using Microsoft.AspNetCore.Components;
using MobSysFinalsBase1.Shared;
using MobSysFinalsBase1.Models;
using BlazorBootstrap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MobSysFinalsBase1.Components.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject]
        public NavigationManager Nav { get; set; }

        [Inject]
        public DatabaseContext DB { get; set; }

        [Inject] protected ToastService ToastService { get; set; } = default!;

        private List<User> contacts = new();
        private string searchText = "";

        private IEnumerable<IGrouping<string, User>> groupedContacts =>
            contacts
                .Where(c => string.IsNullOrWhiteSpace(searchText) ||
                            contactDisplayName(c).Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => contactDisplayName(c))
                .GroupBy(c => contactDisplayName(c).Substring(0, 1).ToUpper());

        protected override async Task OnInitializedAsync()
        {
            if (DB != null)
            {
                await DB.Init();
            }
            await LoadContacts();
            var uri = new Uri(Nav.Uri);
            Debug.WriteLine($"[Home] Current URI: {uri}");
            if (uri.Query.Contains("success=true"))
            {
                Debug.WriteLine("[Home] success=true query parameter detected.");
                try
                {
                    
                    ToastService.Notify(new(ToastType.Success, $"Contact created successfully! 🎉"));
                    Debug.WriteLine("[Home] Toast notification triggered.");
                }
                catch (Exception ex)
                {
                    ToastService.Notify(new(ToastType.Danger, $"Error: {ex.Message}."));
                    Debug.WriteLine($"[Home] Failed to show toast: {ex.Message}");
                }
                Nav.NavigateTo("/", false);
            }
            else
            {
                Debug.WriteLine("[Home] No success=true query parameter found in URI.");
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                await LoadContacts();
            }
        }

        private async Task LoadContacts()
        {
            if (DB != null)
            {
                contacts = await DB.Users();
                StateHasChanged();
            }
        }

        private void NavigateToAddContact()
        {
            Nav.NavigateTo("/add-contact");
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
    }
}
