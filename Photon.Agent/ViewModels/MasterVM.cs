using Photon.Agent.Internal;
using Photon.Agent.Internal.Security;
using Photon.Library.Http.Security;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewModels
{
    internal class MasterVM
    {
        public bool IsSecured {get; set;}
        public HttpUserContext UserContext {get; set;}
        public bool ShowSessions {get; set;}
        public bool ShowVariables {get; set;}
        public bool ShowApplications {get; set;}
        public bool ShowSecurity {get; set;}
        public bool ShowConfiguration {get; set;}


        public void Build(IHttpHandler handler)
        {
            IsSecured = PhotonAgent.Instance.AgentConfiguration.Value.Security?.Enabled ?? false;

            if (IsSecured) {
                var httpSecurity = (HttpSecurityManager)PhotonAgent.Instance.HttpContext.SecurityMgr;

                UserContext = httpSecurity.GetUserContext(handler.HttpContext.Request);
                if (UserContext == null) return;

                var userMgr = PhotonAgent.Instance.UserMgr;

                ShowSessions = userMgr.UserHasRole(UserContext.UserId, GroupRole.SessionView);
                ShowVariables = userMgr.UserHasRole(UserContext.UserId, GroupRole.VariablesView);
                ShowApplications = userMgr.UserHasRole(UserContext.UserId, GroupRole.ApplicationView);
                ShowSecurity = userMgr.UserHasRole(UserContext.UserId, GroupRole.SecurityView);
                ShowConfiguration = userMgr.UserHasRole(UserContext.UserId, GroupRole.ConfigurationView);
            }
            else {
                ShowSessions = true;
                ShowVariables = true;
                ShowApplications = true;
                ShowSecurity = true;
                ShowConfiguration = true;
            }
        }
    }
}
