using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Framework.Server
{
    public class ServerAgent
    {
        public string Id {get; set;}
        public string Name {get; set;}
        public string TcpHost {get; set;}
        public int TcpPort {get; set;}
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
