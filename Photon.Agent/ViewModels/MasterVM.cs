using Photon.Agent.Internal;
using Photon.Agent.Internal.Security;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewModels
{
    internal class MasterVM
    {
        public bool ShowSessions {get; set;}
        public bool ShowVariables {get; set;}
        public bool ShowApplications {get; set;}
        public bool ShowSecurity {get; set;}
        public bool ShowConfiguration {get; set;}


        public void Build(IHttpHandler handler)
        {
            var httpSecurity = (AgentHttpSecurity)PhotonAgent.Instance.HttpContext.SecurityMgr;

            if (!httpSecurity.GetUserContext(handler.HttpContext.Request, out var user))
                return;

            var userId = user.UserId;
            var userMgr = PhotonAgent.Instance.UserMgr;

            ShowSessions = userMgr.UserHasRole(userId, GroupRole.SessionView);
            ShowVariables = userMgr.UserHasRole(userId, GroupRole.VariablesView);
            ShowApplications = userMgr.UserHasRole(userId, GroupRole.ApplicationView);
            ShowSecurity = userMgr.UserHasRole(userId, GroupRole.SecurityView);
            ShowConfiguration = userMgr.UserHasRole(userId, GroupRole.ConfigurationView);
        }
    }
}
