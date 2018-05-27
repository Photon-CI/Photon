using System.Collections.Generic;

namespace Photon.Framework.Packages
{
    public interface IPackageDefinition
    {
        string Id {get; set;}
        string Name {get; set;}
        string Description {get; set;}
        List<PackageFileDefinition> Files {get; set;}
    }
}
