namespace Turnbind
{
    using SharpHook;
    using Turnbind.Model;

    public partial class TurnAction
    {
        public readonly EventSimulator simulator = new();

        public double Interval { get; set; } = 288;

        public IObservable<double> Turn(TurnDirection dir, double pixelPerSec) => new TurnObservable(dir, pixelPerSec, Interval, simulator);
    }
}
