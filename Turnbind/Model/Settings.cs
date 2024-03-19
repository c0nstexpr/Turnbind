using System.IO;
using System.Text.Json;

namespace Turnbind.Model;

public class Settings
{
    public readonly Dictionary<string, KeyBinds> Profiles = [];

    public const string JsonPath = "turn_settings.json";

    public const string DefaultProfileName = "default";

    public Settings() => Load();

    public void Load(string jsonPath = JsonPath)
    {
        if (!File.Exists(jsonPath)) return;

        Profiles.Clear();

        Dictionary<string, KeyBinds> binds;

        {
            using var json = File.OpenRead(jsonPath);
            binds = JsonSerializer.Deserialize<Dictionary<string, KeyBinds>>(json) ?? [];
        }

        foreach (var (name, keyBinds) in binds)
            Profiles[name] = keyBinds;
    }

    public void Save(string jsonPath = JsonPath)
    {
        var json = JsonSerializer.Serialize(Profiles);
        File.WriteAllText(jsonPath, json);
    }
}
