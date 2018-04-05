using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace Photon.Framework.Extensions
{
    public static class JsonSerializerExtensions
    {
        public static void Serialize<T>(this JsonSerializer serializer, Stream stream, T data, bool leaveOpen = false)
        {
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 4096, leaveOpen))
            using (var jsonWriter = new JsonTextWriter(streamWriter)) {
                serializer.Serialize(jsonWriter, data);
            }
        }

        public static T Deserialize<T>(this JsonSerializer serializer, Stream stream, bool leaveOpen = false)
        {
            using (var streamReader = new StreamReader(stream, Encoding.UTF8, true, 4096, leaveOpen))
            using (var jsonReader = new JsonTextReader(streamReader)) {
                return serializer.Deserialize<T>(jsonReader);
            }
        }

        public static dynamic Deserialize(this JsonSerializer serializer, Stream stream, bool leaveOpen = false)
        {
            using (var streamReader = new StreamReader(stream, Encoding.UTF8, true, 4096, leaveOpen))
            using (var jsonReader = new JsonTextReader(streamReader)) {
                return serializer.Deserialize(jsonReader);
            }
        }
    }
}
