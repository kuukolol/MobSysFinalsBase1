using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MobSysFinalsBase1.Models;
using MobSysFinalsBase1.Shared;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MobSysFinalsBase1.Components.Pages
{
    public partial class EditContact : ComponentBase
    {
        [Parameter]
        public int ContactId { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public DatabaseContext DB { get; set; }

        protected User Contact { get; set; } = new User();
        protected string SelectedImagePath { get; set; } = string.Empty;
        protected string UploadErrorMessage { get; set; } = string.Empty;
        protected string DatabaseErrorMessage { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            Debug.WriteLine("[EditContact] OnInitializedAsync started.");
            if (DB != null)
            {
                await DB.Init();
                Debug.WriteLine("[EditContact] Database initialized.");
                await LoadContact();
            }
        }

        private async Task LoadContact()
        {
            if (DB != null)
            {
                var users = await DB.Users();
                Contact = users.FirstOrDefault(u => u.ID == ContactId);
                Debug.WriteLine($"[EditContact] Contact loaded: ID={ContactId}, Name={DisplayName(Contact)}");
                StateHasChanged();
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

                Debug.WriteLine("[EditContact] Contact updated successfully. Navigating to Home with success=true parameter.");
                NavigationManager.NavigateTo("/?success=true&mode=edit");
            }
            catch (Exception ex)
            {
                DatabaseErrorMessage = $"Error updating contact: {ex.Message}";
                Debug.WriteLine($"[EditContact] Error updating contact: {ex.Message}");
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
                        Debug.WriteLine($"[EditContact] File uploaded: {uniqueFileName}");
                    }
                    else
                    {
                        UploadErrorMessage = "Unsupported file type. Only JPG, PNG, and JPEG are allowed.";
                        SelectedImagePath = string.Empty;
                        Debug.WriteLine("[EditContact] Unsupported file type uploaded.");
                    }
                }
            }
            catch (Exception ex)
            {
                UploadErrorMessage = $"Error uploading file: {ex.Message}";
                Debug.WriteLine($"[EditContact] Error uploading file: {ex.Message}");
            }
        }

        protected void Cancel() => NavigationManager.NavigateTo($"/contact/{ContactId}");

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
