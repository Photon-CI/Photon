using Photon.Agent.Internal;
using PiServerLite.Http.Handlers;
using System.Collections.Generic;

namespace Photon.Agent.ViewModels.VariableSet
{
    internal class VariableSetIndexVM : AgentViewModel
    {
        public List<VariableSetItem> Sets {get; set;}


        public VariableSetIndexVM(IHttpHandler handler) : base(handler) {}

        protected override void OnBuild()
        {
            base.OnBuild();

            Sets = new List<VariableSetItem>();

            foreach (var key in PhotonAgent.Instance.Variables.AllKeys) {
                Sets.Add(new VariableSetItem {
                    Id = key,
                });
            }
        }

        internal class VariableSetItem
        {
            public string Id {get; set;}
        }
    }
}
