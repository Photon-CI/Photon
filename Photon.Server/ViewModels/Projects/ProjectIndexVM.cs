using Photon.Framework.Projects;
using Photon.Server.Internal;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Server.ViewModels.Projects
{
    internal class ProjectIndexVM : ViewModelBase
    {
        public List<Project> Projects {get; set;}


        public ProjectIndexVM()
        {
            PageTitle = "Photon Server Projects";
        }

        public override void Build()
        {
            Projects = PhotonServer.Instance.Projects.ToList();
        }
    }
}
