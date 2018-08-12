using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using PiServerLite.Http.Handlers;
using System.Collections.Generic;

namespace Photon.Server.ViewModels.VariableSet
{
    internal class VariablesIndexVM : ServerViewModel
    {
        public List<VariableSet> Sets {get; set;}
        public bool CanUserEdit {get; set;}

        public VariablesIndexVM(IHttpHandler handler) : base(handler) {}

        protected override void OnBuild()
        {
            base.OnBuild();

            CanUserEdit = !Master.IsSecured || PhotonServer.Instance.UserMgr.UserHasRole(Master.UserContext.UserId, GroupRole.VariablesEdit);

            Sets = new List<VariableSet>();

            foreach (var key in PhotonServer.Instance.Variables.AllKeys) {
                Sets.Add(new VariableSet {
                    Id = key,
                });
            }
        }

        internal class VariableSet
        {
            public string Id {get; set;}
        }
    }
}
