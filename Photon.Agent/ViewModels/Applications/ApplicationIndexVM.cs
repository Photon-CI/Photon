using Photon.Agent.Internal;
using PiServerLite.Http.Handlers;
using System.Linq;

namespace Photon.Agent.ViewModels.Applications
{
    internal class ApplicationIndexVM : AgentViewModel
    {
        public ApplicationRow[] Applications {get; set;}


        public ApplicationIndexVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Agent Applications";
        }

        protected override void OnBuild()
        {
            base.OnBuild();

            Applications = PhotonAgent.Instance.ApplicationMgr.All
                .SelectMany(projApps => projApps)
                .Select(app => new ApplicationRow {
                    ProjectId = app.ProjectId,
                    Name = app.Name,
                    RevisionCount = app.Revisions.Count().ToString("N0"),
                }).OrderBy(x => x.Name).ToArray();
        }

        public class ApplicationRow
        {
            public string ProjectId {get; set;}
            public string Name {get; set;}
            public string RevisionCount {get; set;}
        }
    }
}
