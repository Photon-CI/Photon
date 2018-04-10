using Photon.Framework.Packages;

namespace Photon.Framework.Scripts
{
    public interface IServerContext
    {
        string AssemblyFile {get;}
        string WorkDirectory {get;}
        string BinDirectory {get;}
        string ContentDirectory {get;}
        ScriptOutput Output {get;}

        ProjectPackageManager ProjectPackages {get;}
    }
}
