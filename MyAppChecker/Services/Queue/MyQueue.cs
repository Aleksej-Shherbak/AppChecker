using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MyAppChecker.Services.Queue
{
    public class MyQueue: IMyQueue
    {
        private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems = 
            new ConcurrentQueue<Func<CancellationToken, Task>>();
     
        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

        public int Size => _workItems.Count;

        /// <summary>
        /// Добавляет задачу (в виде делегата) в очередь. Метод работает с семафором,
        /// который необходим для того, чтоб не "захватить" задачу в момент ее добавления. 
        /// </summary>
        /// <param name="workItem"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void QueueItem(Func<CancellationToken, Task> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem)); 
            }
            
            _workItems.Enqueue(workItem);
            _signal.Release();
        }

        /// <summary>
        /// Извлекает делегат (задачу) из очереди. 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);

            return workItem;
        }
    }
}