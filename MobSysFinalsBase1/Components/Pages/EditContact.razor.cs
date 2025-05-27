using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MyContact.Models;
using MyContact.Shared;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Web;

namespace MyContact.Components.Pages
{
    public partial class EditContact : ComponentBase
    {
        [Parameter]
        public int ContactId { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public DatabaseContext DB { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        protected User Contact { get; set; } = new User();
        protected string SelectedImagePath { get; set; } = string.Empty;
        protected string UploadErrorMessage { get; set; } = string.Empty;
        protected string DatabaseErrorMessage { get; set; } = string.Empty;

        protected List<Country> Countries { get; set; } = new();
        protected Country SelectedCountry { get; set; }
        protected bool ShowCountryDropdown { get; set; } = false;
        protected string PhoneNumberInput { get; set; } = string.Empty;
        protected string CountrySearch { get; set; } = string.Empty;
        protected ElementReference DropdownElement;

        protected override async Task OnInitializedAsync()
        {
            Countries = CountryData.GetCountries();
            await LoadContact();
        }

        private async Task LoadContact()
        {
            if (DB != null)
            {
                await DB.Init();
                var users = await DB.Users();
                Contact = users.FirstOrDefault(u => u.ID == ContactId);

                if (Contact != null)
                {
                    PhoneNumberInput = Contact.PhoneNumber;
                    string dialCode = "+" + (Contact.CountryCode ?? "63");
                    SelectedCountry = Countries.FirstOrDefault(c => c.DialCode == dialCode) ?? Countries.FirstOrDefault(c => c.DialCode == "+63");
                }
                else
                {
                    SelectedCountry = Countries.FirstOrDefault(c => c.DialCode == "+63");
                }

                StateHasChanged();
            }
        }

        protected IEnumerable<Country> FilteredCountries =>
            string.IsNullOrWhiteSpace(CountrySearch)
                ? Countries
                : Countries.Where(c =>
                    c.Name.StartsWith(CountrySearch, StringComparison.OrdinalIgnoreCase) ||
                    c.Code.StartsWith(CountrySearch, StringComparison.OrdinalIgnoreCase) ||
                    c.DialCode.Contains(CountrySearch));

        protected List<Country> FilteredCountriesList =>
            FilteredCountries.ToList();

        protected async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && FilteredCountries.Any())
            {
                SelectCountry(FilteredCountries.First());
                return;
            }
            else if (e.Key == "Escape")
            {
                ShowCountryDropdown = false;
                CountrySearch = string.Empty;
                StateHasChanged();
                return;
            }
            else if (e.Key.Length == 1 && char.IsLetterOrDigit(e.Key[0]))
            {
                CountrySearch += e.Key;
                StateHasChanged();
            }
            else if (e.Key == "Backspace")
            {
                if (CountrySearch.Length > 0)
                {
                    CountrySearch = CountrySearch.Substring(0, CountrySearch.Length - 1);
                    StateHasChanged();
                }
            }
        }

        protected async Task HandleValidSubmit()
        {
            try
            {
                var imagesDir = Path.Combine(FileSystem.AppDataDirectory, "images");
                if (!Directory.Exists(imagesDir))
                    Directory.CreateDirectory(imagesDir);

                string filePath;
                if (!string.IsNullOrEmpty(SelectedImagePath))
                {
                    filePath = SelectedImagePath;
                }
                else
                {
                    filePath = Contact.ProfilePicture;
                }

                Contact.ProfilePicture = filePath;

                if (SelectedCountry != null && !string.IsNullOrWhiteSpace(PhoneNumberInput))
                {
                    Contact.CountryCode = SelectedCountry.DialCode.Replace("+", "");
                    Contact.PhoneNumber = PhoneNumberInput.TrimStart('0');
                }
                else
                {
                    Contact.CountryCode = string.Empty;
                    Contact.PhoneNumber = PhoneNumberInput;
                }

                if (DatabaseContext.Instance == null)
                {
                    DatabaseErrorMessage = "Database service is not initialized. Please restart the app.";
                    return;
                }

                await DatabaseContext.Instance.SaveUser(Contact);

                var users = await DatabaseContext.Instance.Users();
                int actualId = users.OrderByDescending(u => u.ID).FirstOrDefault()?.ID ?? 1;
                if (!string.IsNullOrEmpty(SelectedImagePath))
                {
                    var ext = Path.GetExtension(filePath).ToLower();
                    var safeName = (Contact.FirstName ?? "user").Trim().ToLower().Replace(" ", "_");
                    var finalFile = Path.Combine(imagesDir, $"{safeName}_{actualId}_avatar{ext}");

                    if (File.Exists(filePath))
                    {
                        if (File.Exists(finalFile) && !string.Equals(finalFile, filePath, StringComparison.OrdinalIgnoreCase))
                            File.Delete(finalFile);

                        File.Move(filePath, finalFile);
                    }

                    Contact.ProfilePicture = finalFile;
                    await DatabaseContext.Instance.SaveUser(Contact);
                }

                NavigationManager.NavigateTo("/?success=true&mode=edit");
            }
            catch (Exception ex)
            {
                DatabaseErrorMessage = $"Error updating contact: {ex.Message}";
            }
        }

        protected async Task HandleFileSelected(InputFileChangeEventArgs e)
        {
            try
            {
                var file = e.File;
                if (file != null)
                {
                    var ext = Path.GetExtension(file.Name).ToLower();
                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                    {
                        var imagesDir = Path.Combine(FileSystem.AppDataDirectory, "images");
                        if (!Directory.Exists(imagesDir))
                            Directory.CreateDirectory(imagesDir);

                        var uniqueFileName = Path.Combine(imagesDir, $"{DateTime.Now.Ticks}_{file.Name}");
                        using var fs = new FileStream(uniqueFileName, FileMode.Create);
                        await file.OpenReadStream().CopyToAsync(fs);
                        SelectedImagePath = uniqueFileName;
                        UploadErrorMessage = string.Empty;
                    }
                    else
                    {
                        UploadErrorMessage = "Unsupported file type. Only JPG, PNG, and JPEG are allowed.";
                        SelectedImagePath = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                UploadErrorMessage = $"Error uploading file: {ex.Message}";
            }
        }

        protected void Cancel() => NavigationManager.NavigateTo($"/contact/{ContactId}");

        protected async Task DeleteContact()
        {
            try
            {
                bool confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this contact?");
                if (!confirmed)
                    return;

                if (Contact == null || Contact.ID == 0)
                {
                    DatabaseErrorMessage = "No contact loaded to delete.";
                    return;
                }

                if (DatabaseContext.Instance == null)
                {
                    DatabaseErrorMessage = "Database service is not initialized. Please restart the app.";
                    return;
                }

                await DatabaseContext.Instance.DeleteUser(Contact);
                NavigationManager.NavigateTo("/?success=true&mode=delete");

            }
            catch (Exception ex)
            {
                DatabaseErrorMessage = $"Error deleting contact: {ex.Message}";
            }
        }

        protected void ToggleCountryDropdown()
        {
            ShowCountryDropdown = !ShowCountryDropdown;
            if (ShowCountryDropdown)
            {
                CountrySearch = string.Empty;
            }
            StateHasChanged();
        }

        protected void SelectCountry(Country country)
        {
            SelectedCountry = country;
            ShowCountryDropdown = false;
            CountrySearch = string.Empty;
            StateHasChanged();
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

        private string GetInitial(User user)
        {
            var name = DisplayName(user);
            return !string.IsNullOrEmpty(name)
                ? name.Substring(0, 1).ToUpper()
                : "?";
        }

        protected string GetImageSrc(string filePath)
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
                catch
                {
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