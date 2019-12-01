using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyAppChecker.Infrastructure;
using MyAppChecker.Services;
using MyAppChecker.Services.Callback;
using MyAppChecker.Services.CheckApp;
using MyAppChecker.Services.Jobs;
using MyAppChecker.Services.Queue;
using Polly;

namespace MyAppChecker
{
    public class Startup
    {
        private readonly ILogger _logger;
        
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            _logger = logger;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var proxy = Configuration.GetValue<string>("Proxy");

            if (string.IsNullOrEmpty(proxy))
            {
                _logger.LogWarning("Невозможно получить URL прокси. Ананимный парсинг невозможен.");
            }

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
           // services.AddHostedService<SchedulerService>();
            services.AddHostedService<MyWorker>();
            services.AddSingleton<IMyQueue, MyQueue>();

            services.AddHttpClient<ICheckPackagesService, CheckPackagesService>(x => { })
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    Proxy = new WebProxy()
                })
                .AddPolicyHandler(PolicySettings.GetRetryPolicy())
                .AddPolicyHandler(PolicySettings.GetTimeoutPolicy());

            Policy.BulkheadAsync(10);

            services.AddHttpClient<ISendCallbackService, SendCallbackService>(x =>
            {
                x.Timeout = TimeSpan.FromSeconds(90);
            });

            services.AddTransient<IRevisionJob, RevisionJob>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                // app.UseHsts();
            }

            //  app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}