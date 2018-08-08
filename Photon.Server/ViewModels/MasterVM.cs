using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ViewModels
{
    internal class MasterVM
    {
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
            var httpSecurity = (ServerHttpSecurity)PhotonServer.Instance.HttpContext.SecurityMgr;

            if (!httpSecurity.GetUserContext(handler.HttpContext.Request, out var user))
                return;

            var userId = user.UserId;
            var userMgr = PhotonServer.Instance.UserMgr;

            ShowAgents = userMgr.UserHasRole(userId, GroupRole.AgentView);
            ShowProjects = userMgr.UserHasRole(userId, GroupRole.ProjectView);
            ShowVariables = userMgr.UserHasRole(userId, GroupRole.VariablesView);
            ShowPackages = userMgr.UserHasRole(userId, GroupRole.PackagesView);
            ShowBuilds = userMgr.UserHasRole(userId, GroupRole.BuildView);
            ShowDeployments = userMgr.UserHasRole(userId, GroupRole.DeployView);
            ShowSecurity = userMgr.UserHasRole(userId, GroupRole.SecurityView);
            ShowConfiguration = userMgr.UserHasRole(userId, GroupRole.ConfigurationView);
        }
    }
}
