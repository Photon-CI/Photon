using Newtonsoft.Json;
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

        [JsonProperty("statusUrl")]
        public string StatusUrl {get; set;}

        [JsonProperty("hookTask")]
        public string HookTaskName {get; set;}

        [JsonProperty("hookRoles")]
        public string[] HookTaskRoles {get; set;}
    }
}
