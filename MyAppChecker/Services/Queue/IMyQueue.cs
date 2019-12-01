using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyAppChecker.Services.Queue
{
    public interface IMyQueue
    {
        int Size { get; }

        void QueueItem(Func<CancellationToken, Task> item);

        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
}