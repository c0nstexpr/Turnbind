using System.Text.Json;
using System.Text.Json.Serialization;

using MoreLinq;

using Turnbind.Helper;

using KeyBindList = System.Collections.Generic.IReadOnlyList<System.Collections.Generic.KeyValuePair<Turnbind.Model.InputKeys, Turnbind.Model.TurnSetting>>;

namespace Turnbind.Model;
public class KeyBindsJsonConverter : JsonConverter<KeyBinds>
{
    public override KeyBinds? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        KeyBinds keyBinds = [];

        options.GetConverter<KeyBindList>().Read(ref reader, options)?
            .ForEach(((ICollection<KeyValuePair<InputKeys, TurnSetting>>)keyBinds).Add);

        return keyBinds;
    }

    public override void Write(
        Utf8JsonWriter writer,
        KeyBinds value,
        JsonSerializerOptions options
    ) => options.GetConverter<KeyBindList>().Write(writer, value.ToArray(), options);
}