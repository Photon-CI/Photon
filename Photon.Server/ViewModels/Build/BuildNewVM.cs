using Photon.Server.Internal;
using System.Linq;

namespace Photon.Server.ViewModels.Build
{
    internal class BuildNewVM : ServerViewModel
    {
        public Framework.Projects.Project[] Projects {get; set;}


        public void Build()
        {
            Projects = PhotonServer.Instance.Projects.All
                .Select(x => x.Description).ToArray();
        }
    }
}
