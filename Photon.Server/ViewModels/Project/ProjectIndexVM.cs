using Photon.Server.Internal;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Server.ViewModels.Project
{
    internal class ProjectIndexVM : ServerViewModel
    {
        public List<Framework.Projects.Project> Projects {get; set;}


        public ProjectIndexVM()
        {
            PageTitle = "Photon Server Projects";
        }

        public void Build()
        {
            Projects = PhotonServer.Instance.Projects.All
                .Select(x => x.Description).ToList();
        }
    }
}
