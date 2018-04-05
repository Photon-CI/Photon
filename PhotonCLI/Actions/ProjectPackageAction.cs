using Photon.Framework.Packages;
using System.Threading.Tasks;

namespace Photon.CLI.Actions
{
    public class ProjectPackageAction
    {
        public string MetadataFilename {get; set;}
        public string PackageFilename {get; set;}
        public string PackageVersion {get; set;}


        public async Task Run()
        {
            await PackageTools.CreateProjectPackage(MetadataFilename, PackageVersion, PackageFilename);
        }
    }
}
