using System;

namespace Photon.Library.Security
{
    public class User
    {
        public string Id {get; set;}
        public bool IsEnabled {get; set;}
        public string Username {get; set;}
        public string Password {get; set;}
        public string DisplayName {get; set;}
        public bool IsDomainEnabled {get; set;}


        public static User New()
        {
            return new User {
                Id = new Guid().ToString("D"),
            };
        }
    }
}
