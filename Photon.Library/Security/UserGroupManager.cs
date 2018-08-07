﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Library.Security
{
    public class UserGroupManager
    {
        public List<UserGroup> Groups {get; set;}
        public List<User> Users {get; set;}


        public UserGroupManager()
        {
            Groups = new List<UserGroup>();
            Users = new List<User>();
        }

        public bool TryGetUser(string username, out User user)
        {
            user = Users.FirstOrDefault(x => string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase));
            return user != null;
        }

        public bool UserHasRole(string userId, string role)
        {
            var userGroups = Groups.Where(x => x.UserIdList.Contains(userId)).ToArray();
            return userGroups.Any(x => x.RoleList.Contains(role, StringComparer.OrdinalIgnoreCase));
        }

        public bool UserHasRoles(string userId, params string[] roles)
        {
            var userGroups = Groups.Where(x => x.UserIdList.Contains(userId)).ToArray();
            return roles.All(role => userGroups.Any(x => x.RoleList.Contains(role, StringComparer.OrdinalIgnoreCase)));
        }
    }
}
