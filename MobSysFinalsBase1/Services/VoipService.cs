using System;
using System.Threading.Tasks;

namespace MobSysFinalsBase1.Services
{
    public class VoipService
    {
        private bool _isInitialized = false;
        private bool _isCallActive = false;

        public async Task InitializeAsync(string sipUsername = "shinononomechan", string sipPassword = "F36I8Ie'wC+3", string sipServer = "sip.linphone.org")
        {
            if (_isInitialized)
                return;

            try
            {
                // Request microphone permission
                var micStatus = await Permissions.RequestAsync<Permissions.Microphone>();
                if (micStatus != PermissionStatus.Granted)
                {
                    Console.WriteLine("Microphone permission denied.");
                    return;
                }

                // Simulate initialization of VoIP library
                Console.WriteLine("Initializing VoIP service...");
                await Task.Delay(500); // Simulate initialization delay
                Console.WriteLine($"VoIP service initialized with SIP account: {sipUsername}@{sipServer}");
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing VoIP service: {ex.Message}");
                _isInitialized = false;
            }
        }

        public async Task MakeCall(string phoneNumber)
        {
            if (!_isInitialized)
            {
                // Using provided SIP credentials for initialization
                await InitializeAsync("shinononomechan", "F36I8Ie'wC+3", "sip.linphone.org");
                if (!_isInitialized)
                    throw new Exception("Failed to initialize VoIP service.");
            }

            try
            {
                // Simulate making a VoIP call
                Console.WriteLine($"Initiating call to {phoneNumber}");
                await Task.Delay(1000); // Simulate connection delay
                Console.WriteLine($"Call connected to {phoneNumber}");
                _isCallActive = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error making call: {ex.Message}");
                throw;
            }
        }

        public void EndCall()
        {
            if (_isCallActive)
            {
                // Simulate ending a VoIP call
                Console.WriteLine("Ending call.");
                _isCallActive = false;
            }
            else
            {
                Console.WriteLine("No active call to end.");
            }
        }

        public void ToggleMute(bool isMuted)
        {
            // Simulate toggling mute
            Console.WriteLine($"Mute toggled: {isMuted}");
        }

        public void ToggleSpeaker(bool isSpeakerOn)
        {
            // Simulate toggling speakerphone
            Console.WriteLine($"Speaker toggled: {isSpeakerOn}");
        }
    }
}
