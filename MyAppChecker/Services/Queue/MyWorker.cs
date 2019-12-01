using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyAppChecker.Services.Queue
{
    public class MyWorker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IMyQueue _myQueue;
        private readonly ILogger _logger;

        public MyWorker(ILogger<MyWorker> logger, IMyQueue myQueue, IConfiguration configuration)
        {
            _logger = logger;
            _myQueue = myQueue;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var workersCount = _configuration.GetValue<int>("WorkersCount");

            if (workersCount == 0)
            {
                throw new ArgumentNullException("Количество воркеров не может быть 0");
            }

            var workers = Enumerable.Range(0, workersCount).Select(num => RunInstance(num, stoppingToken));

            // Запускает одновременно все воркеры
            await Task.WhenAll(workers);
        }

        private async Task RunInstance(int num, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Worker {num} запущен");

            while (!stoppingToken.IsCancellationRequested)
            {
                var jobItem = await _myQueue.DequeueAsync(stoppingToken);

                try
                {
                    _logger.LogInformation("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    _logger.LogInformation($"Задача #{num} взята в обработку. Размер очереди: {_myQueue.Size}");
                    // это делегат, который добавлен из контроллера 
                    await jobItem(stoppingToken);
                    _logger.LogInformation("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    _logger.LogInformation($"Задача #{num} закончена. Размер очереди: {_myQueue.Size}");
                }
                
                catch (Exception e)
                {
                    _logger.LogError($"При работе с задачей {num} произошло исключение. Сообщение:");
                    _logger.LogError(e.Message);
                    if (e.InnerException != null)
                    {
                        _logger.LogError("Внутреннее исключение:");
                        _logger.LogError(e.InnerException.Message);
                    }

                    _logger.LogError(e.StackTrace);
                }
            }
            
            _logger.LogInformation($"Worker {num} остановлен");
        }
    }
}