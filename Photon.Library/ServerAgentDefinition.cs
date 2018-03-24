using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Library
{
    public class ServerAgentDefinition
    {
        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("address")]
        public string Address {get; set;}

        [JsonProperty("roles")]
        public List<string> Roles {get; set;}


        public ServerAgentDefinition()
        {
            Roles = new List<string>();
        }

        public bool MatchesRoles(IEnumerable<string> roles)
        {
            return roles.Any(ContainsRole);
        }

        public bool ContainsRole(string role)
        {
            return Roles.Any(x => string.Equals(x, role, StringComparison.OrdinalIgnoreCase));
        }
    }
}
