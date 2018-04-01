﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Framework
{
    [Serializable]
    public class AgentDefinition
    {
        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("url")]
        public string Url {get; set;}

        [JsonProperty("roles")]
        public List<string> Roles {get; set;}


        public AgentDefinition()
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
