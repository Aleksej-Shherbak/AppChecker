using Newtonsoft.Json;

namespace MyAppChecker.Response
{
    public class CheckPackageResponse
    {
        [JsonProperty("app_id")]
        public string AppId { get; set; }
        [JsonProperty("http_status")]
        public int HttpStatus { get; set; }

    }
}