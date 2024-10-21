using Serilog.Events;

namespace Turnbind.Model;

public struct Settings()
{
    public Dictionary<string, Dictionary<InputKeys, TurnSetting>> Profiles { get; set; } = [];

    public string ProcessName { get; set; } = "";

    public double TurnInterval { get; set; } = 10;

    public ConsoleOption Console { get; set; }

    public LogLevel LogLevel { get; set; }

    public readonly bool EnableConsole => Console switch
    {
        ConsoleOption.Enable => true,
        ConsoleOption.Disable => false,
        ConsoleOption.Default => App.IsInDebug,
        _ => throw new InvalidOperationException()
    };

    public readonly LogEventLevel LogEventLevel => LogLevel switch
    {
        LogLevel.Default => App.IsInDebug ? LogEventLevel.Debug : LogEventLevel.Error,
        LogLevel.Verbose => LogEventLevel.Verbose,
        LogLevel.Debug => LogEventLevel.Debug,
        LogLevel.Information => LogEventLevel.Information,
        LogLevel.Warning => LogEventLevel.Warning,
        LogLevel.Error => LogEventLevel.Error,
        LogLevel.Fatal => LogEventLevel.Fatal,
        _ => throw new InvalidOperationException()
    };
}
