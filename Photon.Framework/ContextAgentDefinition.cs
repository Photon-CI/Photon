using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Framework
{
    public class ContextAgentDefinition
    {
        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("roles")]
        public List<string> Roles {get; set;}


        public bool IsInRole(string roleName)
        {
            return Roles.Any(x => string.Equals(x, roleName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
