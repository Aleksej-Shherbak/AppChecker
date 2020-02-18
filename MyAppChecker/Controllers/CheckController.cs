using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyAppChecker.Infrastructure;
using MyAppChecker.Request;
using MyAppChecker.Services.Jobs;
using MyAppChecker.Services.Queue;

namespace MyAppChecker.Controllers
{
    [DisplayNameValidationFilter]
    public class CheckController : ControllerBase
    {
        private readonly IRevisionJob _revisionJob;
        private readonly IMyQueue _myQueue;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public CheckController(IRevisionJob revisionJob, IMyQueue myQueue, ILogger<CheckController> logger, IConfiguration configuration)
        {
            _revisionJob = revisionJob;
            _myQueue = myQueue;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("/check")]
        public ActionResult Index([FromBody] CheckRequest request)
        {
            _logger.LogInformation("Добавляю задачу ...");

            _myQueue.QueueItem(token => _revisionJob.StartRevision(request));
            
            return Ok(new
            {
                status = "Ok",
                queue_size = _myQueue.Size,
                possible_app_ids = _configuration.GetValue<int>("MaxAppIDsCount"),
                given_number_app_ids = request.AppIds.Count
            });
        }
    }
}