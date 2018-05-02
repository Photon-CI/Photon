using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Photon.Framework.Projects
{
    [Serializable]
    public class ProjectGithubSource : IProjectSource
    {
        [JsonProperty("type")]
        public string Type {get; set;}

        [JsonProperty("username")]
        public string Username {get; set;}

        [JsonProperty("password")]
        public string Password {get; set;}

        [JsonProperty("cloneUrl")]
        public string CloneUrl {get; set;}

        [JsonProperty("hookTask")]
        public string HookTaskName {get; set;}

        [JsonProperty("hookRoles")]
        public string[] HookTaskRoles {get; set;}

        [JsonProperty("notifyOrigin")]
        [JsonConverter(typeof(StringEnumConverter))]
        public NotifyOrigin NotifyOrigin {get; set;}


        public ProjectGithubSource()
        {
            NotifyOrigin = NotifyOrigin.Server;
        }
    }

    public enum NotifyOrigin
    {
        Server,
        Agent,
    }
}
