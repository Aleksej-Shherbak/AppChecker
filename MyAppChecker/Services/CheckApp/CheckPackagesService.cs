using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyAppChecker.Response;

namespace MyAppChecker.Services.CheckApp
{
   public class CheckPackagesService : ICheckPackagesService
    {
        private readonly HttpClient _client;
        readonly ILogger<CheckPackagesService> _log;

        public CheckPackagesService(HttpClient client, ILogger<CheckPackagesService> log)
        {
            _client = client;
            _log = log;
        }

        public async Task<List<CheckPackageResponse>> CheckAsync(List<string> appIds)
        {
            if (!appIds.Any())
            {
                return new List<CheckPackageResponse>();
            }

            _client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2062.124 Safari/537.36");

            _client.DefaultRequestHeaders.ConnectionClose = true;
            
            var sem = new SemaphoreSlim(25);
            
            var tasks = appIds.Select(async (a, i) =>
            {
                //  is it possible to change ip addres here ?  
                
                await sem.WaitAsync(300);

                var response = new CheckPackageResponse
                {
                    AppId = a,
                };

                string url;

                if (a.All(Char.IsDigit))
                {
                    url = $"https://apps.apple.com/ru/app/id{a}?dataOnly=true&isWebExpV2=true";
                }
                else
                {
                    url = $"https://play.google.com/store/apps/details?id={a}";
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    
                    using (var resp = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.HttpStatus = (int) resp.StatusCode;

                        return response;
                    }
                }
                catch (Exception e)
                {
                    _log.LogError(e.Message);
                    _log.LogError("Inner: " + e.InnerException?.Message);
                    return response;
                }
                finally
                {
                    sem.Release();
                }
            }).ToList();
            
            return (await Task.WhenAll(tasks)).ToList();
        }
    }

}