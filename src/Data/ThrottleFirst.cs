using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading;

namespace Data
{
    /// See https://github.com/dotnet/reactive/issues/395
    static class ObservablesEx
    {
        /// See https://github.com/dotnet/reactive/issues/395
        public static IObservable<T> ThrottleFirst<T>(this IObservable<T> source,
                TimeSpan timespan, IScheduler timeSource)
        {
            return new ThrottleFirstObservable<T>(source, timeSource, timespan);
        }
    }

    sealed class ThrottleFirstObservable<T> : IObservable<T>
    {
        readonly IObservable<T> source;

        readonly IScheduler timeSource;

        readonly TimeSpan timespan;

        internal ThrottleFirstObservable(IObservable<T> source,
                  IScheduler timeSource, TimeSpan timespan)
        {
            this.source = source;
            this.timeSource = timeSource;
            this.timespan = timespan;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var parent = new ThrottleFirstObserver(observer, timeSource, timespan);
            var d = source.Subscribe(parent);
            parent.OnSubscribe(d);
            return d;
        }

        sealed class ThrottleFirstObserver : IDisposable, IObserver<T>
        {
            readonly IObserver<T> downstream;

            readonly IScheduler timeSource;

            readonly TimeSpan timespan;

            IDisposable upstream;

            T queued;

            bool once;

            double due;

            internal ThrottleFirstObserver(IObserver<T> downstream,
                    IScheduler timeSource, TimeSpan timespan)
            {
                this.downstream = downstream;
                this.timeSource = timeSource;
                this.timespan = timespan;
            }

            public void OnSubscribe(IDisposable d)
            {
                if (Interlocked.CompareExchange(ref upstream, d, null) != null)
                {
                    d.Dispose();
                }
            }

            public void Dispose()
            {
                var d = Interlocked.Exchange(ref upstream, this);
                if (d != null && d != this)
                {
                    d.Dispose();
                }
            }

            public void OnCompleted()
            {
                downstream.OnCompleted();
            }

            public void OnError(Exception error)
            {
                downstream.OnError(error);
            }
            public void OnNext(T value)
            {
                var now = timeSource.Now.ToUnixTimeMilliseconds();
                if (!once)
                {
                    queued = default(T);
                    once = true;
                    due = now + timespan.TotalMilliseconds;
                    downstream.OnNext(value);
                }
                else if (now >= due)
                {
                    queued = default(T);
                    due = now + timespan.TotalMilliseconds;
                    downstream.OnNext(value);
                }
                else
                {
                    bool firstQueue = queued == null;
                    queued = value;
                    if (firstQueue)
                    {
                        timeSource.Schedule(due - now, (IScheduler s, double d) =>
                        {
                            OnNext(queued);
                            return null;
                        });
                    }
                }
            }
        }
    }
}
