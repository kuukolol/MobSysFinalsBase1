using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MobSysFinalsBase1.Models;
using MobSysFinalsBase1.Shared;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;

namespace MobSysFinalsBase1.Components.Pages
{
    public partial class AddContact : ComponentBase
    {
        protected User NewContact { get; set; } = new User();
        protected string SelectedImagePath { get; set; } = string.Empty;
        protected string UploadErrorMessage { get; set; } = string.Empty;
        protected string DatabaseErrorMessage { get; set; } = string.Empty;

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        protected async Task HandleValidSubmit()
        {
            try
            {
                var imagesDir = Path.Combine(FileSystem.AppDataDirectory, "images");
                if (!Directory.Exists(imagesDir))
                    Directory.CreateDirectory(imagesDir);

                string filePath;
                if (string.IsNullOrEmpty(SelectedImagePath))
                {
                    var rand = new Random();
                    var hexColor = $"#{rand.Next(0, 256):X2}{rand.Next(0, 256):X2}{rand.Next(0, 256):X2}";
                    var svgContent = GetDefaultSvgWithColor(hexColor);
                    filePath = Path.Combine(imagesDir, "temp_avatar.svg");
                    File.WriteAllText(filePath, svgContent);
                }
                else
                {
                    filePath = SelectedImagePath;
                }

                NewContact.ProfilePicture = filePath;

                if (DatabaseContext.Instance == null)
                {
                    DatabaseErrorMessage = "Database service is not initialized. Please restart the app.";
                    return;
                }

                int savedId = await DatabaseContext.Instance.SaveUser(NewContact);

                var users = await DatabaseContext.Instance.Users();
                int actualId = users.OrderByDescending(u => u.ID).FirstOrDefault()?.ID ?? 1;
                var ext = Path.GetExtension(filePath).ToLower();
                var safeName = (NewContact.FirstName ?? "user").Trim().ToLower().Replace(" ", "_");
                var finalFile = Path.Combine(imagesDir, $"{safeName}_{actualId}_avatar{ext}");

                if (File.Exists(filePath))
                {
                    if (File.Exists(finalFile) && !string.Equals(finalFile, filePath, StringComparison.OrdinalIgnoreCase))
                        File.Delete(finalFile);

                    File.Move(filePath, finalFile);
                }

                NewContact.ProfilePicture = finalFile;
                NewContact.ID = actualId;
                await DatabaseContext.Instance.SaveUser(NewContact);

                Debug.WriteLine("[AddContact] Navigating to Home with success=true parameter.");
                NavigationManager.NavigateTo("/?success=true");
            }
            catch (Exception ex)
            {
                DatabaseErrorMessage = $"Error saving contact: {ex.Message}";
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

        protected void Cancel() => NavigationManager.NavigateTo("/");

        private string GetDefaultSvgWithColor(string hexColor)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<svg width=""800px"" height=""800px"" viewBox=""0 0 16 16"" xmlns=""http://www.w3.org/2000/svg"">
  <path d=""m 8 1 c -1.65625 0 -3 1.34375 -3 3 s 1.34375 3 3 3 s 3 -1.34375 3 -3 s -1.34375 -3 -3 -3 z m -1.5 7 c -2.492188 0 -4.5 2.007812 -4.5 4.5 v 0.5 c 0 1.109375 0.890625 2 2 2 h 8 c 1.109375 0 2 -0.890625 2 -2 v -0.5 c 0 -2.492188 -2.007812 -4.5 -4.5 -4.5 z m 0 0"" fill=""{hexColor}""/>
</svg>";
        }
    }
}
