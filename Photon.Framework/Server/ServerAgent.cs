using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Framework.Server
{
    [Serializable]
    public class ServerAgent
    {
        [JsonProperty(Order = 1)]
        public string Id {get; set;}

        [JsonProperty(Order = 2)]
        public string Name {get; set;}

        [JsonProperty(Order = 3)]
        public string TcpHost {get; set;}

        [JsonProperty(Order = 4)]
        public int TcpPort {get; set;}

        [JsonProperty(Order = 5)]
        public List<string> Roles {get; set;}


        public ServerAgent()
        {
            Roles = new List<string>();
        }

        public bool MatchesRoles(IEnumerable<string> roles)
        {
            if (roles == null) throw new ArgumentNullException(nameof(roles));

            return roles.Any(ContainsRole);
        }

        public bool ContainsRole(string role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            return Roles.Any(x => string.Equals(x, role, StringComparison.OrdinalIgnoreCase));
        }
    }
}
