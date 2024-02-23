namespace Turnbind.Model
{
    public class TurnBindKeys
    {
        public TurnDirection Dir { get; set; }

        public List<InputKey> Keys { get; set; } = [];

        public double PixelPerSec { get; set; }
    }
}
