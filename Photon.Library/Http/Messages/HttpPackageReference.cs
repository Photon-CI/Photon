using Newtonsoft.Json;
using Photon.Framework.Packages;

namespace Photon.Library.Http.Messages
{
    public class HttpPackageReference
    {
        [JsonProperty("packageId")]
        public string PackageId {get; set;}

        [JsonProperty("packageVersion")]
        public string PackageVersion {get; set;}


        public HttpPackageReference() {}

        public HttpPackageReference(PackageReference package)
        {
            PackageId = package.PackageId;
            PackageVersion = package.PackageVersion;
        }
    }
}
