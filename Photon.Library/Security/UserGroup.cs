using System.Collections.Generic;

namespace Photon.Library.Security
{
    public class UserGroup
    {
        public List<string> UserIdList {get; set;}
        public List<string> RoleList {get; set;}


        public UserGroup()
        {
            UserIdList = new List<string>();
            RoleList = new List<string>();
        }
    }
}
