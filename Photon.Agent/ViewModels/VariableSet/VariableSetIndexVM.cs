using Photon.Agent.Internal;
using Photon.Library;
using System.Collections.Generic;

namespace Photon.Agent.ViewModels.VariableSet
{
    internal class VariableSetItem
    {
        public string Id {get; set;}
    }

    internal class VariableSetIndexVM : ViewModelBase
    {
        public List<VariableSetItem> Sets {get; set;}


        public void Build()
        {
            Sets = new List<VariableSetItem>();

            foreach (var key in PhotonAgent.Instance.Variables.AllKeys) {
                Sets.Add(new VariableSetItem {
                    Id = key,
                });
            }
        }
    }
}
