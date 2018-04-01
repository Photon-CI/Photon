﻿using Newtonsoft.Json;

namespace Photon.Agent.Internal
{
    public class AgentHttpDefinition
    {
        [JsonProperty("host")]
        public string Host {get; set;}

        [JsonProperty("port")]
        public int Port {get; set;}

        [JsonProperty("path")]
        public string Path {get; set;}
    }
}
