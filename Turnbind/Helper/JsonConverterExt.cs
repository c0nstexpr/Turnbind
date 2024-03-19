using System.Text.Json;
using System.Text.Json.Serialization;

namespace Turnbind.Helper;

public static class JsonConverterExt
{
    public static T? Read<T>(
        this JsonConverter<T> converter,
        ref Utf8JsonReader reader,
        JsonSerializerOptions options
    ) => converter.Read(ref reader, typeof(T), options);
}
