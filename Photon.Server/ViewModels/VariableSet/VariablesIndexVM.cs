using Photon.Server.Internal;
using System.Collections.Generic;

namespace Photon.Server.ViewModels.VariableSet
{
    internal class VariableSet
    {
        public string Id {get; set;}
    }

    internal class VariablesIndexVM : ServerViewModel
    {
        public List<VariableSet> Sets {get; set;}


        public void Build()
        {
            Sets = new List<VariableSet>();

            foreach (var key in PhotonServer.Instance.Variables.AllKeys) {
                Sets.Add(new VariableSet {
                    Id = key,
                });
            }
        }
    }
}
