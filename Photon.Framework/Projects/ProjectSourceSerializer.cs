using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Photon.Framework.Projects
{
    public class ProjectSourceSerializer : JsonConverter
    {
        //public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) =>
            typeof(IProjectSource).IsAssignableFrom(objectType);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject.FromObject(value).WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            var jObject = JObject.Load(reader);
            var type = (string)jObject["type"];

            switch (type.ToLower()) {
                case "fs":
                    return jObject.ToObject<ProjectFileSystemSource>();
                case "github":
                    return jObject.ToObject<ProjectGithubSource>();
                default:
                    throw new ApplicationException($"Unknown source type '{type}'!");
            }
        }
    }
}
