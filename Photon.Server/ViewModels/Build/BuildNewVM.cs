using Newtonsoft.Json;
using Photon.Library.HttpMessages;
using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Server.ViewModels.Build
{
    internal class BuildNewVM : ServerViewModel
    {
        public string ProjectId {get; set;}

        public BuildTaskRow[] BuildTasks {get; private set;}

        public string TaskName {get; set;}
        public string GitRefspec {get; set;}
        public string TaskRoles {get; set;}
        public string PreBuildCommand {get; set;}
        public string AssemblyFilename {get; set;}
        public string ProjectName {get; private set;}
        public string ProjectDescription {get; private set;}
        public uint BuildNumber {get; private set;}
        public string ServerSessionId {get; private set;}
        public bool ProjectFound {get; private set;}
        public string BuildTaskJson {get; private set;}


        public BuildNewVM()
        {
            PageTitle = "Photon Server New Build";
        }

        public void Build()
        {
            if (string.IsNullOrEmpty(ProjectId))
                throw new ApplicationException("Project ID is undefined!");

            if (!PhotonServer.Instance.Projects.TryGetDescription(ProjectId, out var projectDesc))
                throw new ApplicationException($"Project '{ProjectId}' not found!");

            ProjectFound = true;
            ProjectName = projectDesc.Name;
            ProjectDescription = projectDesc.Description;

            BuildTasks = projectDesc.BuildTasks?
                .Select(x => new BuildTaskRow {
                    Name = x.Name,
                    Selected = string.Equals(TaskName, x.Name, StringComparison.OrdinalIgnoreCase),
                }).ToArray();

            if (PreBuildCommand == null)
                PreBuildCommand = projectDesc.PreBuild;

            if (AssemblyFilename == null)
                AssemblyFilename = projectDesc.AssemblyFile;

            BuildTaskJson = projectDesc.BuildTasks != null
                ? JsonConvert.SerializeObject(projectDesc.BuildTasks, Formatting.None) : "{}";
        }

        public void Restore(NameValueCollection form)
        {
            ProjectId = form[nameof(ProjectId)];
            TaskName = form[nameof(TaskName)];
            GitRefspec = form[nameof(GitRefspec)];
            PreBuildCommand = form[nameof(PreBuildCommand)];
            AssemblyFilename = form[nameof(AssemblyFilename)];
            TaskRoles = form[nameof(TaskRoles)];
        }

        public async Task StartBuild()
        {
            if (string.IsNullOrWhiteSpace(TaskName))
                throw new ApplicationException("'Task Name' is undefined!");

            var _roles = TaskRoles.Split(new[] {',', ';'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).ToArray();

            if (!PhotonServer.Instance.Projects.TryGet(ProjectId, out var project))
                throw new ApplicationException($"Project '{ProjectId}' was not found!");

            if (string.IsNullOrEmpty(GitRefspec))
                GitRefspec = "master";

            var build = await project.StartNewBuild();
            build.TaskName = TaskName;
            build.TaskRoles = _roles;
            build.PreBuildCommand = PreBuildCommand;
            build.AssemblyFilename = AssemblyFilename;
            build.GitRefspec = GitRefspec;

            var session = new ServerBuildSession {
                Project = project.Description,
                AssemblyFilename = AssemblyFilename,
                PreBuild = PreBuildCommand,
                TaskName = TaskName,
                GitRefspec = GitRefspec,
                Build = build,
                Roles = _roles,
                Mode = AgentStartModes.Any, // TODO: Add to UI
            };

            build.ServerSessionId = session.SessionId;

            PhotonServer.Instance.Sessions.BeginSession(session);
            PhotonServer.Instance.Queue.Add(session);

            ServerSessionId = session.SessionId;
            BuildNumber = session.Build.Number;
        }

        public class BuildTaskRow
        {
            public string Name {get; set;}
            public bool Selected {get; set;}
        }
    }
}
