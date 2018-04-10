using System;
using System.Threading.Tasks;

namespace Photon.Framework.Packages
{
    [Serializable]
    public class ProjectPackageManager : PackageManagerBase
    {
        public async Task Add(string filename)
        {
            var metadata = await ProjectPackageTools.GetMetadata(filename);
            if (metadata == null) throw new Exception("No metadata file found in package!");

            await base.Add(filename, metadata);
        }
    }
}
