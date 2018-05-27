using Photon.Framework.Packages;
using System.IO;
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
            var rootPath = Path.GetDirectoryName(MetadataFilename);
            var metadata = ProjectPackageTools.LoadDefinition(MetadataFilename);
            await ProjectPackageTools.CreatePackage(metadata, rootPath, PackageVersion, PackageFilename);
        }
    }
}
