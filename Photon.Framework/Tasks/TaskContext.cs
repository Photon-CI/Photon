using System;
using System.Text;

namespace Photon.Framework.Tasks
{
    public interface IAgent
    {
        string ApplicationDirectory {get;}
    }

    public class TaskContext
    {
        //public ContextProjectDefinition Project {get; internal set;}
        public string ProjectName {get; set;}

        public string ReleaseVersion {get; set;}

        public string WorkDirectory {get; set;}

        //public ContextTaskDefinition Task {get; internal set;}

        public ContextAgentDefinition Agent {get; internal set;}

        //public ConcurrentBag<object> Artifacts {get;}
        public StringBuilder Output {get;}


        public TaskContext(IAgent agent)
        {
            //Project = new ContextProjectDefinition();
            //Task = new ContextTaskDefinition();
            Agent = new ContextAgentDefinition();
            //Artifacts = new ConcurrentBag<object>();
            Output = new StringBuilder();
        }

        /// <summary>
        /// Downloads a package from the Server to the specified path.
        /// </summary>
        /// <param name="packageId">The ID of the package.</param>
        /// <param name="packageVersion">The version of the package.</param>
        /// <param name="outputPath">The local path to download the file into.</param>
        /// <returns>The filename of the downloaded package.</returns>
        public string DownloadPackage(string packageId, string packageVersion, string outputPath)
        {
            throw new NotImplementedException();
        }

        public string GetApplicationDirectory(string applicationName, string applicationVersion)
        {
            throw new NotImplementedException();
        }
    }
}
