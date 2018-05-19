using System.Collections.Generic;
using Photon.Library;
using Photon.Server.Internal;

namespace Photon.Server.ViewModels.VariableSet
{
    internal class VariableSet
    {
        public string Id {get; set;}
    }

    internal class VariablesIndexVM : ViewModelBase
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
