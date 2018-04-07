using Photon.Framework.Projects;
using Photon.Framework.Scripts;
using System;

namespace Photon.Framework.Tasks
{
    [Serializable]
    public class AgentBuildContext : IAgentBuildContext
    {
        public Project Project {get; set;}
        public string AssemblyFile {get; set;}
        public string TaskName {get; set;}
        public string GitRefspec {get; set;}
        public string WorkDirectory {get; set;}
        public int BuildNumber {get; set;}
        public ScriptOutput Output {get; set;}


        public void RunCommandLine(string command)
        {
            var result = ProcessRunner.Run(WorkDirectory, command, Output);

            if (result.ExitCode != 0)
                throw new ApplicationException("Process terminated with a non-zero exit code!");
        }

        public void PushProjectPackage(string filename)
        {
            throw new NotImplementedException();
        }
    }
}
