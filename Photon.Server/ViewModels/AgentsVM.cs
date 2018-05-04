using Photon.Framework;
using Photon.Server.Internal;

namespace Photon.Server.ViewModels
{
    internal class AgentsVM : ViewModelBase
    {
        public ServerAgentDefinition[] Agents {get; set;}


        public override void Build()
        {
            base.Build();

            Agents = PhotonServer.Instance.Definition.Definition.Agents.ToArray();
        }
    }
}
