using Photon.Agent.Internal;
using System.Collections.Generic;

namespace Photon.Agent.ViewModels.VariableSet
{
    internal class VariableSetIndexVM : AgentViewModel
    {
        public List<VariableSetItem> Sets {get; set;}


        protected override void OnBuild()
        {
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
