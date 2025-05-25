using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobSysFinalsBase1.Utils
{
    /// <summary>
    /// Shared Class to access Camera, Photo Picker, and etc.
    /// </summary>
    public class DeviceUtilities
    {
        /// <summary>
        /// Picks a photo from the device's gallery.
        /// </summary>
        /// <param name="folderPath">The folder path to save the photo.</param>
        /// <param name="targetFilename">Optional target filename for the saved photo.</param>
        /// <returns>The local file path of the saved photo, or empty string if unsuccessful.</returns>
        public static async Task<string> AddPhoto(string folderPath, string targetFilename = "")
        {
            string resp = "";
            var medStatus = await Permissions.RequestAsync<Permissions.Media>();

            if (medStatus == PermissionStatus.Granted)
            {
                FileResult photo = await MediaPicker.Default.PickPhotoAsync();

                if (photo != null)
                {
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string eventualFilename = string.IsNullOrWhiteSpace(targetFilename) ? DateTime.Now.Ticks + Path.GetExtension(photo.FileName) : targetFilename;
                    string localFilePath = Path.Combine(folderPath, eventualFilename);

                    using Stream sourceStream = await photo.OpenReadAsync();
                    using FileStream localFileStream = File.OpenWrite(localFilePath);

                    await sourceStream.CopyToAsync(localFileStream);

                    resp = localFilePath;
                }
            }

            return resp;
        }

        /// <summary>
        /// Captures a photo using the device's camera.
        /// </summary>
        /// <param name="folderPath">The folder path to save the photo.</param>
        /// <param name="targetFilename">Optional target filename for the saved photo.</param>
        /// <returns>The local file path of the saved photo, or empty string if unsuccessful.</returns>
        public static async Task<string> TakePhoto(string folderPath, string targetFilename = "")
        {
            string resp = "";
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var camStatus = await Permissions.RequestAsync<Permissions.Camera>();
                var micStatus = await Permissions.RequestAsync<Permissions.Microphone>();

                if (camStatus == PermissionStatus.Granted && micStatus == PermissionStatus.Granted)
                {
                    FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

                    if (photo != null)
                    {
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        string eventualFilename = string.IsNullOrWhiteSpace(targetFilename) ? DateTime.Now.Ticks + Path.GetExtension(photo.FileName) : targetFilename;
                        string localFilePath = Path.Combine(folderPath, eventualFilename);
                        using Stream sourceStream = await photo.OpenReadAsync();
                        using FileStream localFileStream = File.OpenWrite(localFilePath);
                        await sourceStream.CopyToAsync(localFileStream);

                        resp = localFilePath;
                    }
                }
            }
            return resp;
        }

        /// <summary>
        /// Initiates a phone call to the specified number using the device's default phone app.
        /// Will prompt the user for phone and microphone permissions if not already granted.
        /// </summary>
        /// <param name="phoneNumber">The phone number to dial.</param>
        /// <returns>True if the call was initiated successfully, false otherwise.</returns>
        public static async Task<bool> MakePhoneCall(string phoneNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    return false;
                }

                var phoneStatus = await Permissions.RequestAsync<Permissions.Phone>();
                var micStatus = await Permissions.RequestAsync<Permissions.Microphone>();
               

                if (phoneStatus == PermissionStatus.Granted && micStatus == PermissionStatus.Granted)
                {
                    if (PhoneDialer.Default.IsSupported)
                    {
                        PhoneDialer.Default.Open(phoneNumber);
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("PhoneDialer is not supported on this device.");
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initiating phone call: {ex.Message}");
                return false;
            }
        }
    }
}
