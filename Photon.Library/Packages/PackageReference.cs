namespace Photon.Library.Packages
{
    public class PackageReference
    {
        public string PackageId {get; set;}
        public string PackageVersion {get; set;}


        public PackageReference() {}

        public PackageReference(string id, string version)
        {
            PackageId = id;
            PackageVersion = version;
        }
    }
}
