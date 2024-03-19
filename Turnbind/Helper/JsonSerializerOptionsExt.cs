using System.Text.Json;
using System.Text.Json.Serialization;

namespace Turnbind.Helper;

public static class JsonSerializerOptionsExt
{
    public static JsonConverter<T> GetConverter<T>(this JsonSerializerOptions options) =>
        (JsonConverter<T>)options.GetConverter(typeof(T));
}
