using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ViewModels.VariableSet
{
    internal class VariablesEditJsonVM : ServerViewModel
    {
        public string SetId {get; set;}
        public bool CanUserEdit {get; set;}

        public bool IsNew => string.IsNullOrEmpty(SetId);


        public VariablesEditJsonVM(IHttpHandler handler) : base(handler) {}

        protected override void OnBuild()
        {
            base.OnBuild();

            CanUserEdit = !Master.IsSecured || PhotonServer.Instance.UserMgr.UserHasRole(Master.UserContext.UserId, GroupRole.VariablesEdit);
        }
    }
}
