using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyAppChecker.Request;
using MyAppChecker.Services.Callback;
using MyAppChecker.Services.CheckApp;

namespace MyAppChecker.Services.Jobs
{
    public class RevisionJob : IRevisionJob
    {
        private readonly ICheckPackagesService _checkPackagesService;
        private readonly ISendCallbackService _callbackService;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public RevisionJob(ICheckPackagesService checkPackagesService, ISendCallbackService callbackService,
            IConfiguration configuration, ILogger<RevisionJob> logger)
        {
            _checkPackagesService = checkPackagesService;
            _callbackService = callbackService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task StartRevision(CheckRequest checkRequest)
        {
            List<string> appIds = checkRequest.AppIds;

            var maxCount = _configuration.GetValue<int>("MaxAppIDsCount");

            if (maxCount == 0)
            {
                _logger.LogWarning("Максимальное количество App id  не выставлено. По-умолчанию будет 500");
                maxCount = 500;
            }

            if (appIds.Count > maxCount)
            {
                appIds = appIds.GetRange(0, 500);
            }

            var checkRes = await _checkPackagesService.CheckAsync(appIds);

            await _callbackService.SendCallbackAsync(checkRes, checkRequest.CallbackUrl);
        }
    }
}