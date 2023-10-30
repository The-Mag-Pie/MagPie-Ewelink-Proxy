using EwelinkNet;
using EwelinkNet.Classes;

namespace MagPie_Ewelink_Proxy.Services
{
    public class EwelinkService
    {
        private static string Username => Environment.GetEnvironmentVariable("EWELINK_USERNAME") ?? string.Empty;
        private static string Password => Environment.GetEnvironmentVariable("EWELINK_PASSWORD") ?? string.Empty;
        private static string AppID => Environment.GetEnvironmentVariable("EWELINK_APP_ID") ?? string.Empty;
        private static string AppSecret => Environment.GetEnvironmentVariable("EWELINK_APP_SECRET") ?? string.Empty;
        private static string Region => Environment.GetEnvironmentVariable("EWELINK_REGION") ?? string.Empty;
        private static string DeviceID => Environment.GetEnvironmentVariable("DEVICE_ID") ?? string.Empty;

        private readonly Ewelink _service;

        private SwitchDevice Device => (SwitchDevice)_service.Devices.Where(d => d.deviceid == DeviceID).Single();

        public EwelinkService()
        {
            _service = new Ewelink(Username, Password, AppID, AppSecret, Region);
            _service.GetCredentials().Wait();
        }

        public async Task<string> GetState()
        {
            await GetDevices();
            return Device.GetState();
        }

        public async Task Toggle()
        {
            await GetDevices();
            Device.Toggle();
        }

        private async Task GetDevices()
        {
            try
            {
                await _service.GetDevices();
            }
            catch
            {
                await _service.GetCredentials();
                await _service.GetDevices();
            }
        }
    }
}
