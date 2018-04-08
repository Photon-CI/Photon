using Photon.Framework.Projects;
using Photon.Framework.Scripts;
using System;

namespace Photon.Framework.Tasks
{
    public class AgentBuildContext : MarshalByRefObject, IAgentBuildContext
    {
        public event EventHandler<PackagePushEventArgs> ProjectPackagePushed;
        public event EventHandler<PackagePushEventArgs> ApplicationPackagePushed;

        public Project Project {get; set;}
        public string AssemblyFile {get; set;}
        public string TaskName {get; set;}
        public string GitRefspec {get; set;}
        public string WorkDirectory {get; set;}
        public string ContentDirectory {get; set;}
        public string BinDirectory {get; set;}
        public int BuildNumber {get; set;}
        public ScriptOutput Output {get; set;}


        public void RunCommandLine(string command)
        {
            Output.Append("Running Command: ", ConsoleColor.DarkCyan)
                .AppendLine(command, ConsoleColor.Cyan);

            var result = ProcessRunner.Run(ContentDirectory, command, Output);

            if (result.ExitCode != 0) {
                Output.Append("Command Failed! Exit Code ", ConsoleColor.DarkYellow)
                    .AppendLine(result.ExitCode.ToString(), ConsoleColor.Yellow);

                throw new ApplicationException("Process terminated with a non-zero exit code!");
            }
        }

        public void PushProjectPackage(string filename)
        {
            OnProjectPackagePushed(filename);
        }

        public void PushApplicationPackage(string filename)
        {
            OnApplicationPackagePushed(filename);
        }

        protected virtual void OnProjectPackagePushed(string filename)
        {
            ProjectPackagePushed?.Invoke(this, new PackagePushEventArgs(filename));
        }

        protected virtual void OnApplicationPackagePushed(string filename)
        {
            ApplicationPackagePushed?.Invoke(this, new PackagePushEventArgs(filename));
        }
    }

    public class PackagePushEventArgs
    {
        public string Filename {get;}

        public PackagePushEventArgs(string filename)
        {
            this.Filename = filename;
        }
    }
}
