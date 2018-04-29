using Newtonsoft.Json;
using System;

namespace Photon.Framework.Projects
{
    [Serializable]
    public class ProjectGithubSource : IProjectSource
    {
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
    }
}
