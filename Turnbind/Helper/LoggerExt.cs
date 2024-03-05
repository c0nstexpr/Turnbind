using System.Runtime.CompilerServices;

using Serilog;

namespace Turnbind.Helper;

static class LoggerExt
{
    public static ILogger WithSourceInfo(this ILogger logger, SourceInfo info) => logger.ForContext(nameof(SourceInfo), info);

    public static ILogger WithSourceInfo(
        this ILogger logger,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    ) => logger.ForContext(nameof(SourceInfo), new SourceInfo(file, member, line));
}
