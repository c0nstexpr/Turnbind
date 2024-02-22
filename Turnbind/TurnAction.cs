namespace Turnbind
{
    using SharpHook;
    using SharpHook.Native;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class TurnAction
    {
        public readonly EventSimulator simulator = new();

        public double Interval { get; set; } = 288;

        public void TurnLeft(double edpi)
        {
            simulator.SimulateMouseMovementRelative(-edpi * Interval, 0);
        }
    }
}
