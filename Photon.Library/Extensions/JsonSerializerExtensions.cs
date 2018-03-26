using Newtonsoft.Json;
using System.IO;

namespace Photon.Library.Extensions
{
    public static class JsonSerializerExtensions
    {
        public static void Serialize<T>(this JsonSerializer serializer, Stream stream, T data)
        {
            using (var streamWriter = new StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(streamWriter)) {
                serializer.Serialize(jsonWriter, data);
            }
        }

        public static T Deserialize<T>(this JsonSerializer serializer, Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(streamReader)) {
                return serializer.Deserialize<T>(jsonReader);
            }
        }
    }
}
