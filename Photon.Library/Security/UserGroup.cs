using Newtonsoft.Json;
using System.Collections.Generic;

namespace Photon.Library.Security
{
    public class UserGroup
    {
        [JsonProperty("id")]
        public string Id {get; set;}

        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("users")]
        public List<string> UserIdList {get; set;}

        [JsonProperty("roles")]
        public List<string> RoleList {get; set;}


        public UserGroup()
        {
            UserIdList = new List<string>();
            RoleList = new List<string>();
        }
    }
}
