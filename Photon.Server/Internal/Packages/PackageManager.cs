using Photon.Framework.Packages;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Packages
{
    internal class PackageManager
    {
        public bool TryGet(string packageId, string packageVersion, out string packageFilename)
        {
            packageFilename = GetPackageFilename(packageId, packageVersion);
            return File.Exists(packageFilename);
        }

        public async Task Add(string filename)
        {
            var metadata = await ProjectPackageTools.GetMetadata(filename);
            if (metadata == null) throw new Exception("No metadata file found in package!");

            var packageFilename = GetPackageFilename(metadata.Id, metadata.Version);

            if (File.Exists(packageFilename))
                throw new Exception($"Package '{metadata.Id}.{metadata.Version}' already exists!");

            await Task.Run(() => {
                var path = Path.GetDirectoryName(packageFilename) ?? string.Empty;

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                File.Copy(filename, packageFilename);
            });
        }

        private static string GetPackageFilename(string packageId, string packageVersion)
        {
            var filename = $"{packageId}.{packageVersion}.zip";
            return Path.Combine(Configuration.ProjectPackageDirectory, packageId, packageVersion, filename);
        }
    }
}
