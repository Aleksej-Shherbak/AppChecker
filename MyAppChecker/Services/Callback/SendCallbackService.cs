using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MyAppChecker.Response;
using Newtonsoft.Json;

namespace MyAppChecker.Services.Callback
{
    public class SendCallbackService : ISendCallbackService
    {
        private readonly HttpClient _client;
        readonly ILogger<SendCallbackService> _log;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public SendCallbackService(HttpClient client, ILogger<SendCallbackService> log,
            IWebHostEnvironment hostingEnvironment)
        {
            _client = client;
            _log = log;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task SendCallbackAsync(List<CheckPackageResponse> checkPackageResponses,
            string callbackUrl)
        {
            var payload = JsonConvert.SerializeObject(new {result = checkPackageResponses});

            try
            {
                HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
                var res = await _client.PostAsync(callbackUrl, httpContent);
                res.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
                _log.LogTrace(e.StackTrace);
                throw new Exception(
                    $"Ошибка при запросе на callback-url, URL: {callbackUrl}. Сообщение об ошибке: {e.Message}");
            }

            _log.LogInformation(payload);
        }
    }

}