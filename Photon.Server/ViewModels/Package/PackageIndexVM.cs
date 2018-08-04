using Photon.Server.Internal;
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


        public PackageIndexVM()
        {
            PageTitle = "Photon Server Packages";
        }

        public void Build()
        {
            ProjectPackages = PhotonServer.Instance
                .ProjectPackages.GetAllPackages().ToArray();
            
            ApplicationPackages = PhotonServer.Instance
                .ApplicationPackages.GetAllPackages().ToArray();
        }
    }
}
