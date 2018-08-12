using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using PiServerLite.Http.Handlers;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Server.ViewModels.Project
{
    internal class ProjectIndexVM : ServerViewModel
    {
        public List<Framework.Projects.Project> Projects {get; set;}
        public bool UserCanEdit {get; set;}


        public ProjectIndexVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Server Projects";
        }

        protected override void OnBuild()
        {
            base.OnBuild();

            UserCanEdit = !Master.IsSecured || PhotonServer.Instance.UserMgr.UserHasRole(Master.UserContext.UserId, GroupRole.ProjectEdit);

            Projects = PhotonServer.Instance.Projects.All
                .Select(x => x.Description).ToList();
        }
    }
}
