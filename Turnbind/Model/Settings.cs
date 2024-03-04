using System.IO;
using System.Text.Json;

namespace Turnbind.Model;

public class Settings
{
    public readonly Dictionary<string, KeyBinds> Profile = [];

    public const string JsonPath = "turn_settings.json";

    public const string DefaultProfileName = "default";

    public string ProfileName { get; set; } = DefaultProfileName;

    public KeyBinds CurrentProfile => Profile[ProfileName];

    public Settings() => Load();

    public void Load(string jsonPath = JsonPath)
    {
        if (!File.Exists(jsonPath)) return;

        Profile.Clear();

        Dictionary<string, KeyBinds> binds;

        {
            using var json = File.OpenRead(jsonPath);
            binds = JsonSerializer.Deserialize<Dictionary<string, KeyBinds>>(json) ?? [];
        }

        foreach (var (name, keyBinds) in binds)
            Profile[name] = keyBinds;
    }

    public void Save(string jsonPath = JsonPath)
    {
        using var json = File.Create(jsonPath);
        JsonSerializer.Serialize(json, Profile);
    }
}
