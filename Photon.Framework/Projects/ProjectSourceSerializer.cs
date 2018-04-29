using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Photon.Framework.Projects
{
    public class ProjectSourceSerializer : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) =>
            typeof(IProjectSource).IsAssignableFrom(objectType);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
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



        //public object GetSourceObject()
        //{
        //    return Source is JObject
        //        ? ParseSource(Source, SourceType)
        //        : Source;
        //}

        //private static object ParseSource(dynamic source, string type)
        //{
        //    switch (type.ToLower()) {
        //        case "github":
        //            return (ProjectGithubSource)source.ToObject<ProjectGithubSource>();
        //        case "fs":
        //            return (ProjectFileSystemSource)source?.ToObject<ProjectFileSystemSource>();
        //        default:
        //            throw new ApplicationException($"Unknown source type '{type}'!");
        //    }
        //}
    }
}
