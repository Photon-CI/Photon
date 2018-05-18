using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Photon.Framework.Projects
{
    [Serializable]
    public class ProjectGithubSource : IProjectSource
    {
        [JsonProperty("type", Order = 1)]
        public string Type {get; set;}

        [JsonProperty("cloneUrl", Order = 2)]
        public string CloneUrl {get; set;}

        [JsonProperty("username", Order = 3)]
        public string Username {get; set;}

        [JsonProperty("password", Order = 4)]
        public string Password {get; set;}

        [JsonProperty("hookTask", Order = 5)]
        public string HookTaskName {get; set;}

        [JsonProperty("hookRoles", Order = 6)]
        public string[] HookTaskRoles {get; set;}

        [JsonProperty("notifyOrigin", Order = 7)]
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
