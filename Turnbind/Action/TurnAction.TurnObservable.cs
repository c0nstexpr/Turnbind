namespace Turnbind
{
    using SharpHook;

    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using Turnbind.Action;
    using Turnbind.Model;

    public partial class TurnAction
    {
        class TurnObservable : IObservable<double>
        {
            readonly Stopwatch _watch = new();

            TimeSpan _preTime;

            readonly IObservable<double> _observable;

            internal TurnObservable(TurnDirection dir, double pixelPerSec, double interval, EventSimulator simulator)
            {
                var p = 0.0;

                _observable = Observable.Interval(TimeSpan.FromSeconds(1) / interval)
                    .Select(
                        i =>
                        {
                            var t = _watch.Elapsed;
                            p += pixelPerSec * (t - _preTime).TotalSeconds;
                            _preTime = t;
                            simulator.Turn(dir, p);

                            var floor = Math.Floor(p);

                            p -= floor;

                            return floor;
                        }
                    );
            }

            public IDisposable Subscribe(IObserver<double> observer)
            {
                _watch.Restart();
                _preTime = _watch.Elapsed;
                var d = _observable.Subscribe(observer);

                return Disposable.Create(
                    () =>
                    {
                        d.Dispose();
                        _watch.Stop();
                    }
                );
            }
        }
    }
}
