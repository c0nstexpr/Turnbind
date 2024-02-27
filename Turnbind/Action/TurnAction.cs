using SharpHook;

using Turnbind.Model;

namespace Turnbind.Action;

public partial class TurnAction
{
    public readonly EventSimulator Simulator = new();

    public double Interval { get; set; } = 288;

    public IObservable<double> Turn(TurnDirection dir, double pixelPerSec) => new TurnObservable(dir, pixelPerSec, Interval, Simulator);
}
