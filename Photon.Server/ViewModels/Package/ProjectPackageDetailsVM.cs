using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System;
using System.IO;
using System.Linq;

namespace Photon.Server.ViewModels.Package
{
    internal class ProjectPackageDetailsVM : ServerViewModel
    {
        public string PackageId {get; set;}
        public PackageRow[] PackageVersions {get; private set;}

        public bool AnyVersions => PackageVersions?.Any() ?? false;


        public ProjectPackageDetailsVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Server Project-Package Details";
        }

        protected override void OnBuild()
        {
            base.OnBuild();

            var packagePath = PhotonServer.Instance
                .ProjectPackages.GetPackagePath(PackageId);

            if (!Directory.Exists(packagePath))
                throw new ApplicationException($"Package '{PackageId}' not found!");

            PackageVersions = Directory.EnumerateFiles(packagePath, $"{PackageId}.*.zip")
                .Select(file => {
                    var name = Path.GetFileNameWithoutExtension(file) ?? string.Empty;
                    var version = name.Substring(PackageId.Length + 1);
                    var created = File.GetCreationTime(file);
                    
                    return new PackageRow {
                        Id = PackageId,
                        Version = version,
                        Created = created.ToString("G"),
                        CreatedValue = created,
                    };
                }).OrderByDescending(x => x.CreatedValue).ToArray();
        }

        public class PackageRow
        {
            public string Id {get; set;}
            public string Version {get; set;}
            public string Created {get; set;}
            public DateTime CreatedValue {get; set;}
        }
    }
}
