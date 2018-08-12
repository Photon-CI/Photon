using Newtonsoft.Json;
using Photon.Framework;
using Photon.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Photon.Library.Security
{
    public class UserGroupManager
    {
        private const string jsonFilename = "UserGroups.json";
        
        //private readonly JsonDynamicDocument serverDocument;
        private DataContainer data;

        public string Directory {get; set;}

        public IEnumerable<UserGroup> AllGroups => data?.Groups;
        public IEnumerable<User> AllUsers => data?.Users;


        public UserGroupManager()
        {
            data = new DataContainer();

            //serverDocument = new JsonDynamicDocument();
        }

        public bool Initialize()
        {
            var filename = Path.Combine(Directory, jsonFilename);

            if (!File.Exists(filename)) return false;

            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read)) {
                data = JsonSettings.Serializer.Deserialize<DataContainer>(stream);
            }
            return true;
        }

        public void Save()
        {
            //serverDocument.Update(Document_OnUpdate);

            var filename = Path.Combine(Directory, jsonFilename);

            using (var stream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write)) {
                JsonSettings.Serializer.Serialize(stream, data);
            }
        }

        public void AddGroup(UserGroup group)
        {
            data.Groups.Add(group);
        }

        public void AddUser(User user)
        {
            data.Users.Add(user);
        }

        public bool RemoveGroup(string groupId)
        {
            return data.Groups.RemoveAll(x => string.Equals(x.Id, groupId)) > 0;
        }

        public bool RemoveUser(string userId)
        {
            return data.Users.RemoveAll(x => string.Equals(x.Id, userId)) > 0;
        }

        public bool TryGetUserById(string id, out User user)
        {
            user = data.Users.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.Ordinal));
            return user != null;
        }

        public bool TryGetUserByUsername(string username, out User user)
        {
            user = data.Users.FirstOrDefault(x => string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase));
            return user != null;
        }

        public bool TryGetGroup(string groupId, out UserGroup group)
        {
            group = data.Groups.FirstOrDefault(x => string.Equals(x.Id, groupId, StringComparison.OrdinalIgnoreCase));
            return group != null;
        }

        public bool UserHasRole(string userId, string role)
        {
            var userGroups = data.Groups.Where(x => x.UserIdList.Contains(userId)).ToArray();
            return userGroups.Any(x => x.RoleList.Contains(role, StringComparer.OrdinalIgnoreCase));
        }

        public bool UserHasRoles(string userId, params string[] roles)
        {
            var userGroups = data.Groups.Where(x => x.UserIdList.Contains(userId)).ToArray();
            return roles.All(role => userGroups.Any(x => x.RoleList.Contains(role, StringComparer.OrdinalIgnoreCase)));
        }

        //private void Document_OnLoad(dynamic document)
        //{
        //    data = document.ToObject<DataContainer>();
        //}

        //private void Document_OnUpdate(dynamic document)
        //{
        //    var mergeValue = JObject.FromObject(data, serverDocument.Serializer);
        //    ((JObject)document).Merge(mergeValue);
        //}

        private class DataContainer
        {
            [JsonProperty("groups")]
            public List<UserGroup> Groups {get;}

            [JsonProperty("users")]
            public List<User> Users {get;}


            public DataContainer()
            {
                Groups = new List<UserGroup>();
                Users = new List<User>();
            }
        }
    }
}
