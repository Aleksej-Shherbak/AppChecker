using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace MyAppChecker.Infrastructure
{
    public static class PolicySettings
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            Random jitterer = new Random();

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg =>
                    msg.StatusCode != HttpStatusCode.NotFound &&
                    msg.StatusCode != HttpStatusCode.OK)
                .Or<HttpRequestException>()
                .Or<OperationCanceledException>()
                .Or<TaskCanceledException>()
                .Or<TimeoutRejectedException>()
                .Or<Exception>()
                .WaitAndRetryAsync(3,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)) +
                               TimeSpan.FromMilliseconds(jitterer.Next(0, 100)),
                    (result, timeSpan, retryCount, context) =>
                    {
                        Console.WriteLine(
                            $"Request failed with {result?.Result?.StatusCode}.  Exception: {result?.Exception?.Message}. Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
                    });
            /*.WaitAndRetryForeverAsync(
                attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))  
                           + TimeSpan.FromMilliseconds(jitterer.Next(0, 100)),
                (exception, retries, time) =>
                {
                    Console.WriteLine($"Count: {retries}, Exception: {exception?.Exception?.Message} time: {time}");
                })*/
        }

        public static AsyncTimeoutPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(80);
        }
    }
}