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

            ProjectPackages = PhotonServer.Instance
                .ProjectPackages.GetAllPackages().ToArray();
            
            ApplicationPackages = PhotonServer.Instance
                .ApplicationPackages.GetAllPackages().ToArray();
        }
    }
}
