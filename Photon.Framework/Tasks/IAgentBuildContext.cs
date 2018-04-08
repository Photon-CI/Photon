using Photon.Framework.Projects;
using Photon.Framework.Scripts;

namespace Photon.Framework.Tasks
{
    public interface IAgentBuildContext
    {
        Project Project {get;}
        string AssemblyFile {get;}
        string TaskName {get;}
        string GitRefspec {get;}
        string WorkDirectory {get;}
        string ContentDirectory {get;}
        string BinDirectory {get;}
        int BuildNumber {get;}
        ScriptOutput Output {get;}

        void RunCommandLine(string command);
        void PushProjectPackage(string filename);
        void PushApplicationPackage(string filename);
    }
}
