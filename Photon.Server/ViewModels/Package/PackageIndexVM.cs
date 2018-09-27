using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System.Linq;

namespace Photon.Server.ViewModels.Package
{
    internal class PackageIndexVM : ServerViewModel
    {
        public string[] ProjectPackages {get; private set;}
        public string[] ApplicationPackages {get; private set;}

        public bool AnyProjectPackages => ProjectPackages?.Any() ?? false;
        public bool AnyApplicationPackages => ApplicationPackages?.Any() ?? false;
        public bool AnyPackages => AnyProjectPackages || AnyApplicationPackages;


        public PackageIndexVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Server Packages";
        }

        protected override void OnBuild()
        {
            base.OnBuild();

            var serverContext = PhotonServer.Instance.Context;
            ProjectPackages = serverContext.ProjectPackages.GetAllPackages().ToArray();
            ApplicationPackages = serverContext.ApplicationPackages.GetAllPackages().ToArray();
        }
    }
}
