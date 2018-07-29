﻿using Newtonsoft.Json;

namespace Photon.Server.Internal.ServerConfiguration
{
    public class ServerSecurityConfiguration
    {
        [JsonProperty("enabled")]
        public bool Enabled {get; set;}

        [JsonProperty("type")]
        public string Type {get; set;}
    }
}
