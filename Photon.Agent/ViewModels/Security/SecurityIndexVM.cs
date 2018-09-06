using Photon.Agent.Internal;
using Photon.Agent.Internal.Security;
using Photon.Library.Security;
using PiServerLite.Http.Handlers;
using System.Linq;

namespace Photon.Agent.ViewModels.Security
{
    internal class SecurityIndexVM : AgentViewModel
    {
        public bool IsDomainEnabled {get; set;}
        public bool UserCanEdit {get; set;}
        public UserGroup[] UserGroups {get; set;}
        public User[] Users {get; set;}


        public SecurityIndexVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Agent Security";
        }

        protected override void OnBuild()
        {
            base.OnBuild();

            UserCanEdit = !Master.IsSecured || PhotonAgent.Instance.UserMgr.UserHasRole(Master.UserContext.UserId, GroupRole.SecurityEdit);

            UserGroups = PhotonAgent.Instance.UserMgr.AllGroups.OrderBy(x => x.Name).ToArray();
            Users = PhotonAgent.Instance.UserMgr.AllUsers.OrderBy(x => x.DisplayName).ToArray();

            IsDomainEnabled = PhotonAgent.Instance.AgentConfiguration.Value.Security.DomainEnabled;
        }
    }
}
