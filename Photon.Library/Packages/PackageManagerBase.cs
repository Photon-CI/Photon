using Photon.Framework.Packages;
using Photon.Framework.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Library.Packages
{
    [Serializable]
    public abstract class PackageManagerBase
    {
        public string PackageDirectory {get; set;}


        public bool TryGet(string packageId, string packageVersion, out string packageFilename)
        {
            packageFilename = GetPackageFilename(packageId, packageVersion);
            return File.Exists(packageFilename);
        }

        public string GetPackagePath(string packageId)
        {
            return Path.Combine(PackageDirectory, packageId);
        }

        protected async Task Add(string filename, IPackageMetadata metadata)
        {
            var packageFilename = GetPackageFilename(metadata.Id, metadata.Version);

            if (File.Exists(packageFilename))
                throw new Exception($"Package '{metadata.Id}.{metadata.Version}' already exists!");

            await Task.Run(() => {
                PathEx.CreateFilePath(packageFilename);

                File.Copy(filename, packageFilename);
            });
        }

        protected string GetPackageFilename(string packageId, string packageVersion)
        {
            var filename = $"{packageId}.{packageVersion}.zip";
            return Path.Combine(PackageDirectory, packageId, filename);
        }

        /// <summary>
        /// Gets a collection of all Package IDs found on the file system.
        /// </summary>
        public IEnumerable<string> GetAllPackages(string searchPattern = "*")
        {
            return Directory.EnumerateDirectories(PackageDirectory, searchPattern)
                .Select(Path.GetFileName);
        }

        /// <summary>
        /// Gets a collection of all versions of the specified package.
        /// </summary>
        public IEnumerable<string> GetAllPackageVersions(string packageId)
        {
            var packagePath = Path.Combine(PackageDirectory, packageId);

            if (!Directory.Exists(packagePath))
                throw new ApplicationException($"Package '{packageId}' not found!");

            return Directory.EnumerateFiles(packagePath, $"{packageId}.*.zip")
                .Select(Path.GetFileNameWithoutExtension)
                .Select(x => x.Substring(packageId.Length + 1));
        }
    }
}
