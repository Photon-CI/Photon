using Newtonsoft.Json;
using System;

namespace Photon.Library.Security
{
    public class User
    {
        [JsonProperty("id")]
        public string Id {get; set;}

        [JsonProperty("enabled")]
        public bool IsEnabled {get; set;}

        [JsonProperty("username")]
        public string Username {get; set;}

        [JsonProperty("password")]
        public string Password {get; set;}

        [JsonProperty("displayName")]
        public string DisplayName {get; set;}

        [JsonProperty("domainEnabled")]
        public bool IsDomainEnabled {get; set;}


        public static User New()
        {
            return new User {
                Id = new Guid().ToString("D"),
            };
        }
    }
}
