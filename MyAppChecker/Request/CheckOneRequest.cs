using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MyAppChecker.Request
{
    public class CheckOneRequest
    {
        [Required(ErrorMessage = "app_id is required")]
        [FromQuery(Name = "app_id")]
        public string AppId { get; set; }
    }
}