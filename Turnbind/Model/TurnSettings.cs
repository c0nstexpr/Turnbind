using System.IO;
using System.Text.Json;

namespace Turnbind.Model;
public class TurnSettings
{
    public HashSet<Binding> Binds { get; } = [];

    public const string JsonPath = "turn_settings.json";

    public TurnSettings() => Load();

    public void Load(string jsonPath = JsonPath)
    {
        if (!File.Exists(jsonPath)) return;

        Binding[] binds;
        using (var json = File.OpenRead(jsonPath))
        {
            binds = JsonSerializer.Deserialize<Binding[]>(json) ?? [];
        }

        Binds.Clear();

        foreach (var bind in binds)
            Binds.Add(bind);
    }

    public void Save(string jsonPath = JsonPath)
    {
        using var json = File.Create(jsonPath);
        JsonSerializer.Serialize(json, Binds);
    }
}
