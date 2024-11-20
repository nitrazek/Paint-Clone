using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Paint_Clone.FileFormatsMode.Utils;

public class TaskQueue
{
    private readonly BlockingCollection<Action> _tasksQueue = new BlockingCollection<Action>();
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly Dispatcher _dispatcher;

    public TaskQueue(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
        StartQueueProcessing();
    }

    public void EnqueueTask(Action task)
    {
        _tasksQueue.Add(task);
    }

    private void StartQueueProcessing()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                var taskToExecute = _tasksQueue.Take();
                await _semaphore.WaitAsync();

                await _dispatcher.InvokeAsync(taskToExecute);
                _semaphore.Release();
            }
        });
    }
}
