using System.Text.Json.Serialization;

namespace Turnbind.Model;

[JsonConverter(typeof(KeyBindsJsonConverter))]
public class KeyBinds : Dictionary<InputKeys, TurnSetting>
{
}