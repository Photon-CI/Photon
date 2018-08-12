using Photon.Framework.Extensions;
using Photon.Library.Security;
using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using IHttpHandler = PiServerLite.Http.Handlers.IHttpHandler;

namespace Photon.Server.ViewModels.Security
{
    internal class SecurityEditGroupVM : ServerViewModel
    {
        public string GroupId {get; set;}
        public string GroupName {get; set;}
        public User[] GroupUsers {get; set;}
        public bool RoleAgentView {get; set;}
        public bool RoleAgentEdit {get; set;}
        public bool RoleProjectView {get; set;}
        public bool RoleProjectEdit {get; set;}
        public bool RoleBuildView {get; set;}
        public bool RoleBuildStart {get; set;}
        public bool RoleBuildDelete {get; set;}
        public bool RoleDeploymentView {get; set;}
        public bool RoleDeploymentStart {get; set;}
        public bool RoleDeploymentDelete {get; set;}
        public bool RoleVariablesView {get; set;}
        public bool RoleVariablesEdit {get; set;}
        public bool RolePackagesView {get; set;}
        public bool RolePackagesDelete {get; set;}
        public bool RoleSecurityView {get; set;}
        public bool RoleSecurityEdit {get; set;}
        public bool RoleConfigurationView {get; set;}
        public bool RoleConfigurationEdit {get; set;}
        public User[] UserList {get; set;}
        public string GroupUserIdList {get; set;}


        public SecurityEditGroupVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Server Edit Group Security";
        }

        protected override void OnBuild()
        {
            base.OnBuild();

            var userMgr = PhotonServer.Instance.UserMgr;
            UserList = userMgr.AllUsers.ToArray();

            if (string.IsNullOrEmpty(GroupId)) return;

            if (!PhotonServer.Instance.UserMgr.TryGetGroup(GroupId, out var group))
                throw new ApplicationException($"Group '{GroupId}' not found!");

            bool HasRole(string role) => group.RoleList.Contains(role, StringComparer.OrdinalIgnoreCase);

            GroupName = group.Name;
            RoleAgentView = HasRole(GroupRole.AgentView);
            RoleAgentEdit = HasRole(GroupRole.AgentEdit);
            RoleProjectView = HasRole(GroupRole.ProjectView);
            RoleProjectEdit = HasRole(GroupRole.ProjectEdit);
            RoleBuildView = HasRole(GroupRole.BuildView);
            RoleBuildStart = HasRole(GroupRole.BuildStart);
            RoleBuildDelete = HasRole(GroupRole.BuildDelete);
            RoleDeploymentView = HasRole(GroupRole.DeployView);
            RoleDeploymentStart = HasRole(GroupRole.DeployStart);
            RoleDeploymentDelete = HasRole(GroupRole.DeployDelete);
            RoleVariablesView = HasRole(GroupRole.VariablesView);
            RoleVariablesEdit = HasRole(GroupRole.VariablesEdit);
            RolePackagesView = HasRole(GroupRole.PackagesView);
            RolePackagesDelete = HasRole(GroupRole.PackagesDelete);
            RoleSecurityView = HasRole(GroupRole.SecurityView);
            RoleSecurityEdit = HasRole(GroupRole.SecurityEdit);
            RoleConfigurationView = HasRole(GroupRole.ConfigurationView);
            RoleConfigurationEdit = HasRole(GroupRole.ConfigurationEdit);

            GroupUsers = group.UserIdList.Select(userId => {
                return userMgr.TryGetUserById(userId, out var user)
                    ? user
                    : new User {
                        Id = userId,
                        DisplayName = userId,
                    };
            }).ToArray();
        }

        public void Restore(NameValueCollection form)
        {
            GroupId = form.Get(nameof(GroupId));
            GroupName = form.Get(nameof(GroupName));
            RoleAgentView = form.Get(nameof(RoleAgentView)).To<bool>();
            RoleAgentEdit = form.Get(nameof(RoleAgentEdit)).To<bool>();
            RoleProjectView = form.Get(nameof(RoleProjectView)).To<bool>();
            RoleProjectEdit = form.Get(nameof(RoleProjectEdit)).To<bool>();
            RoleBuildView = form.Get(nameof(RoleBuildView)).To<bool>();
            RoleBuildStart = form.Get(nameof(RoleBuildStart)).To<bool>();
            RoleBuildDelete = form.Get(nameof(RoleBuildDelete)).To<bool>();
            RoleDeploymentView = form.Get(nameof(RoleDeploymentView)).To<bool>();
            RoleDeploymentStart = form.Get(nameof(RoleDeploymentStart)).To<bool>();
            RoleDeploymentDelete = form.Get(nameof(RoleDeploymentDelete)).To<bool>();
            RoleVariablesView = form.Get(nameof(RoleVariablesView)).To<bool>();
            RoleVariablesEdit = form.Get(nameof(RoleVariablesEdit)).To<bool>();
            RolePackagesView = form.Get(nameof(RolePackagesView)).To<bool>();
            RolePackagesDelete = form.Get(nameof(RolePackagesDelete)).To<bool>();
            RoleSecurityView = form.Get(nameof(RoleSecurityView)).To<bool>();
            RoleSecurityEdit = form.Get(nameof(RoleSecurityEdit)).To<bool>();
            RoleConfigurationView = form.Get(nameof(RoleConfigurationView)).To<bool>();
            RoleConfigurationEdit = form.Get(nameof(RoleConfigurationEdit)).To<bool>();
            GroupUserIdList = form.Get(nameof(GroupUserIdList));
        }

        public void Save()
        {
            var userMgr = PhotonServer.Instance.UserMgr;

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
            if (RoleAgentView) yield return GroupRole.AgentView;
            if (RoleAgentEdit) yield return GroupRole.AgentEdit;
            if (RoleProjectView) yield return GroupRole.ProjectView;
            if (RoleProjectEdit) yield return GroupRole.ProjectEdit;
            if (RoleBuildView) yield return GroupRole.BuildView;
            if (RoleBuildStart) yield return GroupRole.BuildStart;
            if (RoleBuildDelete) yield return GroupRole.BuildDelete;
            if (RoleDeploymentView) yield return GroupRole.DeployView;
            if (RoleDeploymentStart) yield return GroupRole.DeployStart;
            if (RoleDeploymentDelete) yield return GroupRole.DeployDelete;
            if (RoleVariablesView) yield return GroupRole.VariablesView;
            if (RoleVariablesEdit) yield return GroupRole.VariablesEdit;
            if (RolePackagesView) yield return GroupRole.PackagesView;
            if (RolePackagesDelete) yield return GroupRole.PackagesDelete;
            if (RoleSecurityView) yield return GroupRole.SecurityView;
            if (RoleSecurityEdit) yield return GroupRole.SecurityEdit;
            if (RoleConfigurationView) yield return GroupRole.ConfigurationView;
            if (RoleConfigurationEdit) yield return GroupRole.ConfigurationEdit;
        }
    }
}
