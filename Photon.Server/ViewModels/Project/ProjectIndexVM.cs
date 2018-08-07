using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Server.ViewModels.Project
{
    internal class ProjectIndexVM : ServerViewModel
    {
        public List<Framework.Projects.Project> Projects {get; set;}


        public ProjectIndexVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Server Projects";
        }

        protected override void OnBuild()
        {
            base.OnBuild();

            Projects = PhotonServer.Instance.Projects.All
                .Select(x => x.Description).ToList();
        }
    }
}
