using Photon.Library.Security;
using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using PiServerLite.Http.Handlers;
using System.Linq;

namespace Photon.Server.ViewModels.Security
{
    internal class SecurityIndexVM : ServerViewModel
    {
        public bool IsDomainEnabled {get; set;}
        public bool UserCanEdit {get; set;}
        public UserGroup[] UserGroups {get; set;}
        public User[] Users {get; set;}


        public SecurityIndexVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Server Security";
        }

        protected override void OnBuild()
        {
            base.OnBuild();

            UserCanEdit = !Master.IsSecured || PhotonServer.Instance.UserMgr.UserHasRole(Master.UserContext.UserId, GroupRole.SecurityEdit);

            UserGroups = PhotonServer.Instance.UserMgr.AllGroups.ToArray();
            Users = PhotonServer.Instance.UserMgr.AllUsers.ToArray();

            IsDomainEnabled = PhotonServer.Instance.ServerConfiguration.Value.Security.DomainEnabled;
        }
    }
}
