using SharpHook;

using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Turnbind.Model;

namespace Turnbind.Action;

public partial class TurnAction
{
    class TurnObservable : IObservable<double>
    {
        readonly Stopwatch m_watch = new();

        TimeSpan m_preTime;

        readonly IObservable<double> m_observable;

        internal TurnObservable(TurnDirection dir, double pixelPerSec, double interval, EventSimulator simulator)
        {
            var p = 0.0;

            m_observable = Observable.Interval(TimeSpan.FromSeconds(1) / interval)
                .Select(
                    i =>
                    {
                        var t = m_watch.Elapsed;
                        p += pixelPerSec * (t - m_preTime).TotalSeconds;
                        m_preTime = t;
                        simulator.Turn(dir, p);

                        var floor = Math.Floor(p);

                        p -= floor;

                        return floor;
                    }
                );
        }

        public IDisposable Subscribe(IObserver<double> observer)
        {
            m_watch.Restart();
            m_preTime = m_watch.Elapsed;
            var d = m_observable.Subscribe(observer);

            return Disposable.Create(
                () =>
                {
                    d.Dispose();
                    m_watch.Stop();
                }
            );
        }
    }
}
