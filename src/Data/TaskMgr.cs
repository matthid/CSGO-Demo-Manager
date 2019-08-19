using System.Linq;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace BackgroundTasks {
    using Data;
    using TaskId = System.Guid;
    public interface IProgressReporter
    {
        /// Value between 0-100
        void SetProgress(double progress);
        /// Add a message
        void AddMessage(string message);
    }

    public static class ProgressReporterExtensions {
        public static void AddError(this IProgressReporter reporter, string message)
        {
            reporter.AddMessage(message);
        }
    }

    public interface IBackgroundTask {
        TaskId Id { get; }
        string Name { get; }
        DateTime StartTime { get; }
        bool CanCancel { get; }

        IEnumerable<string> Messages { get; }
        bool IsFinished { get; }

        // Value between 0-100.0
        double Progress { get; }
        void Cancel();
    }

    public static class BackgroundTaskExtensions {
        public static string GetLastMessage(this IBackgroundTask task)
        {
            return task.Messages.Last();
        }
    }

    public interface IBackgroundTaskManager {
        IEnumerable<IBackgroundTask> CurrentTasks { get; }
        TaskId StartTask(Func<IProgressReporter, CancellationToken, Task> createTask, string name, bool canCancel = false);
        bool CancelTask(TaskId id);

        bool TryGetTask(TaskId taskId, out IBackgroundTask task);

        IObservable<(TaskId id, double progress)> TaskProgressChanged { get; }
        IObservable<(TaskId id, string message)> TaskMessageChanged { get; }
        IObservable<IBackgroundTask> TaskStarted { get; }
        IObservable<TaskId> TaskCompleted { get; }
    }

    public sealed class BackgroundTask : IBackgroundTask, IProgressReporter, IDisposable
    {
        CancellationTokenSource tokSource = new CancellationTokenSource();
        private readonly ILogger<BackgroundTaskManager> _logger;
        BackgroundTaskManager manager;
        Task _task = null;
        private ConcurrentQueue<string> _messages = new ConcurrentQueue<string>();
        private readonly Subject<double> _taskProgressChanged = new Subject<double>();

        public BackgroundTask(ILogger<BackgroundTaskManager> logger, BackgroundTaskManager mgr, string name, bool canCancel)
        {
            Name = name;
            Id = System.Guid.NewGuid();
            _logger = logger;
            manager = mgr;
            StartTime = DateTime.Now;
            CanCancel = canCancel;
            _sub = _taskProgressChanged.ThrottleFirst(TimeSpan.FromSeconds(1), System.Reactive.Concurrency.Scheduler.Default).Subscribe(newProgress => mgr.OnProgessChanged(this.Id, newProgress));
            Progress = 0;
        }

        public void Dispose()
        {
            _sub.Dispose();
        }

        public string Name { get; }
        public TaskId Id { get; }
        public DateTime StartTime { get; }
        public bool CanCancel { get; }

        private IDisposable _sub;

        public IEnumerable<string> Messages => _messages;
        public bool IsFinished => _task?.IsCompleted ?? false;
        public void Cancel() => tokSource.Cancel();
        public double Progress { get; private set; }

        internal void StartTask(Func<IProgressReporter, CancellationToken, Task> f, SemaphoreSlim maxTasks)
        {
            if (_task != null)
            {
                throw new InvalidOperationException("StartTask can only be called once! Create a new BackgroundTask!");
            }

            manager.OnStarted(this);
            _task = Task.Run(async () =>
            {
                try
                {
                    AddMessage($"Waiting for the task to be queued...");
                    await maxTasks.WaitAsync();
                    AddMessage($"Starting task...");
                    await f(this, tokSource.Token);
                }
                finally
                {
                    maxTasks.Release();
                }
            }, tokSource.Token).ContinueWith(t =>
            {
                if (!tokSource.IsCancellationRequested)
                {
                    SetProgress(100);
                }

                if (t.IsFaulted && t.Exception != null)
                {
                    _logger.LogError(t.Exception, "Error in background task");
                    AddMessage($"Faulted: {t.Exception}");
                }

                manager.OnCompleted(Id);
            });
        }
        
        public void SetProgress(double progress)
        {
            Progress = progress;
            _taskProgressChanged.OnNext(progress);
        }

        public void AddMessage(string message)
        {
            _messages.Enqueue(message);
            manager.OnMessageChanged(Id, message);
        }
    }

    public class BackgroundTaskManager : IBackgroundTaskManager
    {
        private readonly ConcurrentDictionary<TaskId, IBackgroundTask> _tasks = new ConcurrentDictionary<TaskId, IBackgroundTask>();
        
        private readonly Subject<(TaskId id, double progress)> _taskProgressChanged = new Subject<(TaskId id, double progress)>();
        private readonly Subject<(TaskId id, string message)> _taskMessageChanged = new Subject<(TaskId id, string message)>();
        private readonly Subject<TaskId> _taskCompleted = new Subject<TaskId>();
        private readonly Subject<IBackgroundTask> _taskStarted = new Subject<IBackgroundTask>();
        private readonly ILogger<BackgroundTaskManager> _logger;
        private readonly SemaphoreSlim maxThread = new SemaphoreSlim(Math.Max(Environment.ProcessorCount - 3, 3)); // at least 3

        public BackgroundTaskManager(ILogger<BackgroundTaskManager> logger)
        {
            _logger = logger;
        }

        public IEnumerable<IBackgroundTask> CurrentTasks => _tasks.Values;


        internal void OnProgessChanged(TaskId task, double progress)
        {
            _taskProgressChanged.OnNext((task, progress));
            Console.WriteLine($"Task {task} changed to {progress} %");
        }
        internal void OnMessageChanged(TaskId task, string message)
        {
            _taskMessageChanged.OnNext((task, message));
            Console.WriteLine($"Task {task}: {message}");
        }
        internal void OnCompleted(TaskId task) {
            _taskCompleted.OnNext(task);
            _tasks.TryRemove(task, out var t);
            (t as IDisposable)?.Dispose();
        }
        internal void OnStarted(IBackgroundTask task) => _taskStarted.OnNext(task);
        public IObservable<(TaskId id, double progress)> TaskProgressChanged => _taskProgressChanged;
        public IObservable<(TaskId id, string message)> TaskMessageChanged => _taskMessageChanged;
        public IObservable<TaskId> TaskCompleted => _taskCompleted;
        public IObservable<IBackgroundTask> TaskStarted => _taskStarted;
        public TaskId StartTask(Func<IProgressReporter, CancellationToken, Task> createTask, string name, bool canCancel = false)
        {
            var wrappedTask = new BackgroundTask(_logger, this, name, canCancel);
            //_tasks.AddOrUpdate(wrappedTask.Id, ((k, res) => res), (k, t, res) => res, wrappedTask);
            _tasks.AddOrUpdate(wrappedTask.Id, ((k) => wrappedTask), (k, t) => wrappedTask);
            wrappedTask.StartTask(createTask, maxThread);
            return wrappedTask.Id;
        }

        public bool TryGetTask(TaskId taskId, out IBackgroundTask task){
            return _tasks.TryGetValue(taskId, out task);
        }

        public bool CancelTask(TaskId id){
            if (_tasks.TryGetValue(id, out var task))
            {
                task.Cancel();
                return true;
            } 
            else
            {
                _logger.LogWarning("Task {id} unknown", id);
                return false;
            }
        }
    }
}