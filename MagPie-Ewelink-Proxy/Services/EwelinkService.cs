using MagPie_Ewelink_Proxy.Models;
using System.Text;
using System.Text.Json;

namespace MagPie_Ewelink_Proxy.Services
{
    public class EwelinkService
    {
        private static string Nonce => Utils.GenerateRandomString(8);

        private static string Email { get; } = Environment.GetEnvironmentVariable("EWELINK_EMAIL") ?? string.Empty;
        private static string Password { get; } = Environment.GetEnvironmentVariable("EWELINK_PASSWORD") ?? string.Empty;
        private static string CountryCode { get; } = Environment.GetEnvironmentVariable("EWELINK_COUNTRY_CODE") ?? string.Empty;
        private static string Region { get; } = Environment.GetEnvironmentVariable("EWELINK_REGION") ?? string.Empty;
        private static string AppId { get; } = Environment.GetEnvironmentVariable("EWELINK_APP_ID") ?? string.Empty;
        private static string AppSecret { get; } = Environment.GetEnvironmentVariable("EWELINK_APP_SECRET") ?? string.Empty;

        private static bool IsEnvironmentVariablesSet => !string.IsNullOrEmpty(Email) &&
                                                         !string.IsNullOrEmpty(Password) &&
                                                         !string.IsNullOrEmpty(CountryCode) &&
                                                         !string.IsNullOrEmpty(Region) &&
                                                         !string.IsNullOrEmpty(AppId) &&
                                                         !string.IsNullOrEmpty(AppSecret);

        private readonly HttpClient _httpClient;
        private readonly string _apiUrlDomain = "{{region}}-apia.coolkit.cc";
        private string _accessToken;

        public EwelinkService(HttpClient httpClient)
        {
            if (IsEnvironmentVariablesSet == false)
            {
                Console.Error.WriteLine("Environment variables not set");
                Environment.Exit(1);
            }

            _httpClient = httpClient;
            _apiUrlDomain = _apiUrlDomain.Replace("{{region}}", Region);

            ConfigureHttpClient();
            SetTokens();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-CK-Nonce", Nonce);
            _httpClient.DefaultRequestHeaders.Add("Host", _apiUrlDomain);
        }

        private void SetTokens()
        {
            var data = new Dictionary<string, string>()
            {
                { "countryCode", CountryCode },
                { "email", Email },
                { "password", Password }
            };

            var dataJson = JsonSerializer.Serialize(data);
            var sign = Utils.CalculateHMACSignature(dataJson, AppSecret);
            var url = $"https://{_apiUrlDomain}/v2/user/login";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Headers =
                {
                    { "X-CK-Appid", AppId },
                    { "Authorization", $"Sign {sign}" }
                },
                Content = new StringContent(dataJson, Encoding.UTF8, "application/json")
            };

            var response = _httpClient.Send(request);
            if (response.IsSuccessStatusCode == false)
            {
                Console.Error.WriteLine($"Failed to get tokens (HTTP error {response.StatusCode})");
                Environment.Exit(1);
            }

            var responseJson = response.Content.ReadAsStringAsync().Result;
            var responseData = JsonSerializer.Deserialize<EwelinkApiResponse>(responseJson);
            if (responseData is null)
            {
                Console.Error.WriteLine($"Failed to get tokens (invalid response: {responseJson})");
                Environment.Exit(1);
            }
            else if (responseData.Error != 0)
            {
                Console.Error.WriteLine($"Failed to get tokens (error {responseData.Error}: {responseData.Message})");
                Environment.Exit(1);
            }

            var loginInfo = responseData.Data;
            _accessToken = loginInfo.GetProperty("at").GetString() ?? string.Empty;
        }

        public async Task<string> GetState(string deviceId) => await GetState(deviceId, 1);

        private async Task<string> GetState(string deviceId, int recursionCounter)
        {
            var url = $"https://{_apiUrlDomain}/v2/device/thing/status?type=1&id={deviceId}&params=switch";

            var request = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Headers =
                {
                    { "Authorization", $"Bearer {_accessToken}" }
                }
            };

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode == false)
            {
                Console.Error.WriteLine($"Failed to get device state (HTTP error {response.StatusCode})");
                throw new InvalidResponseException();
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<EwelinkApiResponse>(responseJson);
            if (responseData is null)
            {
                Console.Error.WriteLine($"Failed to get device state (invalid response: {responseJson})");
                throw new InvalidResponseException();
            }
            else if (responseData.Error != 0 && recursionCounter < 4)
            {
                SetTokens();
                return await GetState(deviceId, recursionCounter + 1);
            }
            else if (recursionCounter >= 4)
            {
                Console.Error.WriteLine($"Failed to get device state (error {responseData.Error}: {responseData.Message})");
                throw new InvalidResponseException();
            }

            return responseData.Data.GetProperty("params").GetProperty("switch").GetString() ?? string.Empty;
        }

        public async Task Toggle(string deviceId) => await Toggle(deviceId, 1);

        private async Task Toggle(string deviceId, int recursionCounter)
        {
            var state = await GetState(deviceId) == "on" ? "off" : "on";

            var data = new Dictionary<string, dynamic>()
            {
                { "type", 1 },
                { "id", deviceId },
                { "params", new Dictionary<string, string>() { { "switch", state } } }
            };

            var dataJson = JsonSerializer.Serialize(data);
            var url = $"https://{_apiUrlDomain}/v2/device/thing/status";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Headers =
                {
                    { "Authorization", $"Bearer {_accessToken}" }
                },
                Content = new StringContent(dataJson, Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode == false)
            {
                Console.Error.WriteLine($"Failed to set device state (HTTP error {response.StatusCode})");
                throw new InvalidResponseException();
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<EwelinkApiResponse>(responseJson);
            if (responseData is null)
            {
                Console.Error.WriteLine($"Failed to set device state (invalid response: {responseJson})");
                throw new InvalidResponseException();
            }
            else if (responseData.Error != 0 && recursionCounter < 4)
            {
                SetTokens();
                await Toggle(deviceId, recursionCounter + 1);
            }
            else if (recursionCounter >= 4)
            {
                Console.Error.WriteLine($"Failed to set device state (error {responseData.Error}: {responseData.Message})");
                throw new InvalidResponseException();
            }
        }
    }

    public class InvalidResponseException : Exception
    {
        public InvalidResponseException()
        {
        }

        public InvalidResponseException(string message)
            : base(message)
        {
        }

        public InvalidResponseException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
