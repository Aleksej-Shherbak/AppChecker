using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace MyAppChecker.Request
{
    public class CheckRequest
    {
        [DisplayName("app_ids")]
        [JsonProperty("app_ids")]
        [Required(ErrorMessage = "List of app_ids can not be empty")]
        public List<string> AppIds { get; set; }

        [DisplayName("callback_url")]
        [JsonProperty("callback_url")]
        [Required(ErrorMessage = "Callback url can not be empty")]
        public string CallbackUrl { get; set; }
    }
}