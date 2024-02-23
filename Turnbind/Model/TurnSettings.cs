namespace Turnbind.Model
{
    using System.IO;
    using System.Text.Json;

    public class TurnSettings
    {
        public List<TurnBindKeys> Binds { get; } = [];

        public const string JsonPath = "turn_settings.json";

        public TurnSettings() => Load();

        public void Load(string jsonPath = JsonPath)
        {
            if (!File.Exists(jsonPath)) return;

            TurnBindKeys[] binds;
            using (var json = File.OpenRead(jsonPath))
            {
                binds = JsonSerializer.Deserialize<TurnBindKeys[]>(json) ?? [];
            }

            Binds.Clear();
            Binds.Capacity = binds.Length;
            Binds.AddRange(binds);
        }

        public void Save(string jsonPath = JsonPath)
        {
            using var json = File.Create(jsonPath);
            JsonSerializer.Serialize(json, Binds);
        }
    }
}
