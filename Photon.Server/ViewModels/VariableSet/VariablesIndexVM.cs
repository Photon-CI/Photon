using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System.Collections.Generic;

namespace Photon.Server.ViewModels.VariableSet
{
    internal class VariablesIndexVM : ServerViewModel
    {
        public List<VariableSet> Sets {get; set;}


        public VariablesIndexVM(IHttpHandler handler) : base(handler) {}

        protected override void OnBuild()
        {
            base.OnBuild();

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
