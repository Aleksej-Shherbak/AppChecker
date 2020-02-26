using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyAppChecker.Request;
using MyAppChecker.Services.CheckApp;
using MyAppChecker.Services.Jobs;
using MyAppChecker.Services.Queue;

namespace MyAppChecker.Controllers
{
    public class CheckController : ControllerBase
    {
        private readonly IRevisionJob _revisionJob;
        private readonly IMyQueue _myQueue;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly ICheckPackagesService _checkPackagesService;

        public CheckController(IRevisionJob revisionJob, IMyQueue myQueue, ILogger<CheckController> logger,
            IConfiguration configuration, ICheckPackagesService checkPackagesService)
        {
            _revisionJob = revisionJob;
            _myQueue = myQueue;
            _logger = logger;
            _configuration = configuration;
            _checkPackagesService = checkPackagesService;
        }

        [Route("/")]
        public async Task<IActionResult> Index(CheckOneRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var list = new List<string>()
            {
                request.AppId
            };

            var res = (await _checkPackagesService.CheckAsync(list)).First();

            return Ok(res);
        }

        [HttpPost]
        [Route("/check")]
        public ActionResult Check([FromBody] CheckRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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