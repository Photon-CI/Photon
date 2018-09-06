using Photon.Agent.Internal;
using Photon.Agent.Internal.Security;
using Photon.Framework.Extensions;
using Photon.Library.Security;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using IHttpHandler = PiServerLite.Http.Handlers.IHttpHandler;

namespace Photon.Agent.ViewModels.Security
{
    internal class SecurityEditGroupVM : AgentViewModel
    {
        public string GroupId {get; set;}
        public string GroupName {get; set;}
        public User[] GroupUsers {get; set;}
        public bool RoleSessionView {get; set;}
        public bool RoleSessionEdit {get; set;}
        public bool RoleVariablesView {get; set;}
        public bool RoleVariablesEdit {get; set;}
        public bool RoleApplicationView {get; set;}
        public bool RoleApplicationEdit {get; set;}
        public bool RoleSecurityView {get; set;}
        public bool RoleSecurityEdit {get; set;}
        public bool RoleConfigurationView {get; set;}
        public bool RoleConfigurationEdit {get; set;}
        public User[] UserList {get; set;}
        public string GroupUserIdList {get; set;}
        public string SearchText {get; set;}


        public SecurityEditGroupVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Agent Edit Group Security";
        }

        protected override void OnBuild()
        {
            base.OnBuild();

            var userMgr = PhotonAgent.Instance.UserMgr;
            UserList = userMgr.AllUsers.OrderBy(x => x.DisplayName).ToArray();

            if (string.IsNullOrEmpty(GroupId)) return;

            if (!PhotonAgent.Instance.UserMgr.TryGetGroup(GroupId, out var group))
                throw new ApplicationException($"Group '{GroupId}' not found!");

            bool HasRole(string role) => group.RoleList.Contains(role, StringComparer.OrdinalIgnoreCase);

            GroupName = group.Name;
            RoleSessionView = HasRole(GroupRole.SessionView);
            RoleSessionEdit = HasRole(GroupRole.SessionEdit);
            RoleVariablesView = HasRole(GroupRole.VariablesView);
            RoleVariablesEdit = HasRole(GroupRole.VariablesEdit);
            RoleApplicationView = HasRole(GroupRole.ApplicationView);
            RoleApplicationEdit = HasRole(GroupRole.ApplicationEdit);
            RoleSecurityView = HasRole(GroupRole.SecurityView);
            RoleSecurityEdit = HasRole(GroupRole.SecurityEdit);
            RoleConfigurationView = HasRole(GroupRole.ConfigurationView);
            RoleConfigurationEdit = HasRole(GroupRole.ConfigurationEdit);

            GroupUsers = group.UserIdList
                .Select(userId => userMgr.TryGetUserById(userId, out var user)
                    ? user : new User {
                        Id = userId,
                        DisplayName = userId,
                    })
                .OrderBy(x => x.DisplayName).ToArray();
        }

        public void Restore(NameValueCollection form)
        {
            GroupId = form.Get(nameof(GroupId));
            GroupName = form.Get(nameof(GroupName));
            RoleSessionView = form.Get(nameof(RoleSessionView)).To<bool>();
            RoleSessionEdit = form.Get(nameof(RoleSessionEdit)).To<bool>();
            RoleVariablesView = form.Get(nameof(RoleVariablesView)).To<bool>();
            RoleVariablesEdit = form.Get(nameof(RoleVariablesEdit)).To<bool>();
            RoleApplicationView = form.Get(nameof(RoleApplicationView)).To<bool>();
            RoleApplicationEdit = form.Get(nameof(RoleApplicationEdit)).To<bool>();
            RoleSecurityView = form.Get(nameof(RoleSecurityView)).To<bool>();
            RoleSecurityEdit = form.Get(nameof(RoleSecurityEdit)).To<bool>();
            RoleConfigurationView = form.Get(nameof(RoleConfigurationView)).To<bool>();
            RoleConfigurationEdit = form.Get(nameof(RoleConfigurationEdit)).To<bool>();
            GroupUserIdList = form.Get(nameof(GroupUserIdList));
            SearchText = form.Get(nameof(SearchText));
        }

        public void Save()
        {
            var userMgr = PhotonAgent.Instance.UserMgr;

            UserGroup group = null;
            if (!string.IsNullOrEmpty(GroupId)) {
                userMgr.TryGetGroup(GroupId, out group);
            }
            else {
                GroupId = Guid.NewGuid().ToString("D");
            }

            if (group == null) {
                group = new UserGroup {
                    Id = GroupId,
                };

                userMgr.AddGroup(group);
            }

            group.Name = GroupName;
            group.RoleList = GetRoles().ToList();

            group.UserIdList = GroupUserIdList
                .Split(new[] {'&'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(HttpUtility.UrlDecode).ToList();

            userMgr.Save();
        }

        private IEnumerable<string> GetRoles()
        {
            if (RoleSessionView) yield return GroupRole.SessionView;
            if (RoleSessionEdit) yield return GroupRole.SessionEdit;
            if (RoleVariablesView) yield return GroupRole.VariablesView;
            if (RoleVariablesEdit) yield return GroupRole.VariablesEdit;
            if (RoleApplicationView) yield return GroupRole.ApplicationView;
            if (RoleApplicationEdit) yield return GroupRole.ApplicationEdit;
            if (RoleSecurityView) yield return GroupRole.SecurityView;
            if (RoleSecurityEdit) yield return GroupRole.SecurityEdit;
            if (RoleConfigurationView) yield return GroupRole.ConfigurationView;
            if (RoleConfigurationEdit) yield return GroupRole.ConfigurationEdit;
        }
    }
}
