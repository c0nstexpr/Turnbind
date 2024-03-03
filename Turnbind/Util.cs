using Serilog;
using Serilog.Configuration;
using Serilog.Formatting.Compact;

namespace Turnbind;

public static class Util
{
    public static Dictionary<U, T> ToInvDictionary<T, U>(
        this IEnumerable<KeyValuePair<T, U>> keyValuePairs
    ) where U : notnull =>
        keyValuePairs.Select(kv => new KeyValuePair<U, T>(kv.Value, kv.Key)).ToDictionary();

    public static ILogger GetLogger<T>(Action<LoggerSinkConfiguration>? action = null, string? fileName = null)
    {
        var logger = new LoggerConfiguration().MinimumLevel
            .Information()
            .WriteTo
            .File(
                new RenderedCompactJsonFormatter(),
                $"Logs/{fileName ?? typeof(T).Name}.json",
                rollOnFileSizeLimit: true
            );

        action?.Invoke(logger.WriteTo);

        return logger.CreateLogger();
    }
}
