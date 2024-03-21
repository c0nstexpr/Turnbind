using System.IO;
using System.Text.Json;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Turnbind.Model;

public partial class Settings : ObservableObject
{
    public Dictionary<string, KeyBinds> Profiles { get; set; } = [];

    [ObservableProperty]
    string m_processName = "";

    [ObservableProperty]
    double m_turnInterval = 10;

    public const string JsonPath = "turn_settings.json";

    public const string DefaultProfileName = "default";

    public Settings()
    {
    }

    public static Settings? Load(string jsonPath = JsonPath)
    {
        if (!File.Exists(jsonPath)) return null;

        using var json = File.OpenRead(jsonPath);
        return JsonSerializer.Deserialize<Settings>(json);
    }

    public void Save(string jsonPath = JsonPath)
    {
        var json = JsonSerializer.Serialize(this, options: new() { WriteIndented = true });
        File.WriteAllText(jsonPath, json);
    }
}
