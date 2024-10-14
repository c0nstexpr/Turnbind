using System.Text.Json;
using System.Text.Json.Serialization;

using Turnbind.Helper;

using InputKeyList = System.Collections.Generic.IReadOnlyList<Turnbind.Model.InputKey>;

namespace Turnbind.Model;

public class InputKeysJsonConverter : JsonConverter<InputKeys>
{
    public override InputKeys Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => new(options.GetConverter<InputKeyList>().Read(ref reader, options) ?? []);

    public override void Write(
        Utf8JsonWriter writer,
        InputKeys value,
        JsonSerializerOptions options
    ) => options.GetConverter<InputKeyList>().Write(writer, value, options);
}