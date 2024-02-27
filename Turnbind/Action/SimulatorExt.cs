using SharpHook;

using Turnbind.Model;

namespace Turnbind.Action;

internal static class SimulatorExt
{
    public static void Turn(this EventSimulator simulator, TurnDirection dir, double p)
    {
        switch (dir)
        {
            case TurnDirection.Left:
                for (p = -p; p < short.MinValue; p -= short.MinValue)
                    simulator.SimulateMouseMovementRelative(short.MinValue, 0);

                simulator.SimulateMouseMovementRelative((short)p, 0);

                break;
            case TurnDirection.Right:
                for (; p > short.MaxValue; p -= short.MaxValue)
                    simulator.SimulateMouseMovementRelative(short.MaxValue, 0);

                simulator.SimulateMouseMovementRelative((short)p, 0);
                break;
        }
    }
}
