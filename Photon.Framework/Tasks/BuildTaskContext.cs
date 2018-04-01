﻿using Photon.Framework.Projects;
using Photon.Framework.Scripts;
using System;

namespace Photon.Framework.Tasks
{
    public class BuildTaskContext : TaskContextBase
    {
        public ProjectDefinition Project {get; set;}
        public ProjectJobDefinition Job {get; set;}
        public ContextAgentDefinition Agent {get; set;}
        public string TaskName {get; set;}


        public BuildTaskContext(AgentDefinition agent, string sessionId) : base(sessionId) // IAgent agent
        {
            //this.Project = project ?? throw new ArgumentNullException(nameof(project));
            //this.Job = job ?? throw new ArgumentNullException(nameof(job));
            //this.Agent = agent ?? throw new ArgumentNullException(nameof(agents));

            Agent = new ContextAgentDefinition {
                Name = agent.Name,
                Roles = agent.Roles,
            };
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