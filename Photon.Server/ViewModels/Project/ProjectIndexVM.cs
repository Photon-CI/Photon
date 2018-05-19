using System.Collections.Generic;
using System.Linq;
using Photon.Library;
using Photon.Server.Internal;

namespace Photon.Server.ViewModels.Project
{
    internal class ProjectIndexVM : ViewModelBase
    {
        public List<Framework.Projects.Project> Projects {get; set;}


        public ProjectIndexVM()
        {
            PageTitle = "Photon Server Projects";
        }

        public void Build()
        {
            Projects = PhotonServer.Instance.Projects.All.ToList();
        }
    }
}
