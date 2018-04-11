using System;
using System.Threading.Tasks;

namespace Photon.Framework.Packages
{
    [Serializable]
    public class ApplicationPackageManager : PackageManagerBase
    {
        public async Task Add(string filename)
        {
            var metadata = await ApplicationPackageTools.GetMetadata(filename);
            if (metadata == null) throw new Exception("No metadata file found in package!");

            await base.Add(filename, metadata);
        }
    }
}
