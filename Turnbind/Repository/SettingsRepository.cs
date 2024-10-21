using System.IO;
using System.Text.Json;

using Turnbind.Model;

namespace Turnbind.Repository;

class SettingsRepository
{
    public const string SettingsFilePath = "settings.json";

    public Settings Settings { get; } = Read();

    static Settings Read()
    {
        if (File.Exists(SettingsFilePath))
        {
            using var json = File.Open(SettingsFilePath, FileMode.OpenOrCreate);
            return JsonSerializer.Deserialize<Settings>(json);
        }

        return new Settings();
    }

    public void Save()
    {
        using var json = File.OpenWrite(SettingsFilePath);
        JsonSerializer.Serialize(json, Settings);
    }
}
