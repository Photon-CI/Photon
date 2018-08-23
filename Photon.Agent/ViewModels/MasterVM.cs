using Photon.Agent.Internal;
using Photon.Agent.Internal.Security;
using Photon.Library.Http.Security;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewModels
{
    internal class MasterVM
    {
        public bool IsSecured {get; set;}
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

                if (!httpSecurity.TryGetUserContext(handler.HttpContext.Request, out var user))
                    return;

                var userId = user.UserId;
                var userMgr = PhotonAgent.Instance.UserMgr;

                ShowSessions = userMgr.UserHasRole(userId, GroupRole.SessionView);
                ShowVariables = userMgr.UserHasRole(userId, GroupRole.VariablesView);
                ShowApplications = userMgr.UserHasRole(userId, GroupRole.ApplicationView);
                ShowSecurity = userMgr.UserHasRole(userId, GroupRole.SecurityView);
                ShowConfiguration = userMgr.UserHasRole(userId, GroupRole.ConfigurationView);
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
