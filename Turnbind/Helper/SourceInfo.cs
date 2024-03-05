using System.Runtime.CompilerServices;

namespace Turnbind.Helper;

readonly struct SourceInfo(
    [CallerFilePath] string file = "",
    [CallerMemberName] string member = "",
    [CallerLineNumber] int line = 0
)
{
    public readonly string File = file;

    public readonly string Member = member;

    public readonly int Line = line;

    public override string ToString() => $"{File}:{Line}({Member})";
}
