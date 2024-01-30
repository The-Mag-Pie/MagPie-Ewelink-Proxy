using System.Text.Json;
using System.Text.Json.Serialization;

namespace MagPie_Ewelink_Proxy.Models
{
    public class EwelinkApiResponse
    {
        [JsonPropertyName("error")]
        public int Error { get; set; }

        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }

        [JsonPropertyName("msg")]
        public string Message { get; set; }
    }
}
