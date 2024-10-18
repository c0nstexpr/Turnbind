using Serilog.Events;

namespace Turnbind.Model;

public class Settings
{
    public Dictionary<string, Dictionary<InputKeys, TurnSetting>> Profiles { get; set; } = [];

    public string ProcessName { get; set; } = "";

    public double TurnInterval { get; set; }

    public bool Console { get; set; }

    public LogEventLevel LogLevel { get; set; }
}
