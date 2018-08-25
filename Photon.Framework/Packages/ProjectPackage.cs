namespace Photon.Framework.Packages
{
    public class ProjectPackage : IPackageMetadata
    {
        public string Id {get; set;}
        public string Name {get; set;}
        public string Version {get; set;}
        public string Description {get; set;}
        public string Project {get; set;}
        public string AssemblyFilename {get; set;}
        public string ScriptName {get; set;}
    }
}
