using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Photon.Framework.Projects
{
    [Serializable]
    public class Project
    {
        public string Id {get; set;}
        public string Name {get; set;}
        public string Description {get; set;}
        public string SourceType {get; set;}
        public string PreBuild {get; set;}
        public object Source {get; set;}

        [JsonProperty("assembly")]
        public string AssemblyFile {get; set;}


        public object GetSourceObject()
        {
            return Source is JObject
                ? ParseSource(Source, SourceType)
                : Source;
        }

        private static object ParseSource(dynamic source, string type)
        {
            switch (type.ToLower()) {
                case "github":
                    return (ProjectGithubSource)source.ToObject<ProjectGithubSource>();
                case "fs":
                    return (ProjectFileSystemSource)source?.ToObject<ProjectFileSystemSource>();
                default:
                    throw new ApplicationException($"Unknown source type '{type}'!");
            }
        }
    }
}
