using Photon.Library.HttpSecurity;
using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ViewModels
{
    internal class MasterVM
    {
        public bool IsSecured {get; set;}
        public HttpUserContext UserContext {get; set;}
        public bool ShowAgents {get; set;}
        public bool ShowProjects {get; set;}
        public bool ShowVariables {get; set;}
        public bool ShowPackages {get; set;}
        public bool ShowBuilds {get; set;}
        public bool ShowDeployments {get; set;}
        public bool ShowSecurity {get; set;}
        public bool ShowConfiguration {get; set;}


        public void Build(IHttpHandler handler)
        {
            IsSecured = PhotonServer.Instance.ServerConfiguration.Value.Security?.Enabled ?? false;

            if (IsSecured) {
                var httpSecurity = (HttpSecurityManager)PhotonServer.Instance.HttpContext.SecurityMgr;

                UserContext = httpSecurity.GetUserContext(handler.HttpContext.Request);
                if (UserContext == null) return;

                var userMgr = PhotonServer.Instance.UserMgr;

                ShowAgents = userMgr.UserHasRole(UserContext.UserId, GroupRole.AgentView);
                ShowProjects = userMgr.UserHasRole(UserContext.UserId, GroupRole.ProjectView);
                ShowVariables = userMgr.UserHasRole(UserContext.UserId, GroupRole.VariablesView);
                ShowPackages = userMgr.UserHasRole(UserContext.UserId, GroupRole.PackagesView);
                ShowBuilds = userMgr.UserHasRole(UserContext.UserId, GroupRole.BuildView);
                ShowDeployments = userMgr.UserHasRole(UserContext.UserId, GroupRole.DeployView);
                ShowSecurity = userMgr.UserHasRole(UserContext.UserId, GroupRole.SecurityView);
                ShowConfiguration = userMgr.UserHasRole(UserContext.UserId, GroupRole.ConfigurationView);
            }
            else {
                ShowAgents = true;
                ShowProjects = true;
                ShowVariables = true;
                ShowPackages = true;
                ShowBuilds = true;
                ShowDeployments = true;
                ShowSecurity = true;
                ShowConfiguration = true;
            }
        }
    }
}
