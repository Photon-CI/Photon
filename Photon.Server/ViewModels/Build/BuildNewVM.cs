﻿using Photon.Library.HttpMessages;
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
        public bool ProjectFound {get; private set;}
        public string ProjectName {get; private set;}
        public string GitRefspec {get; private set;}
        public string PreBuildCommand {get; private set;}
        public string AssemblyFilename {get; private set;}
        public string TaskName {get; private set;}
        public string TaskRoles {get; private set;}
        public uint BuildNumber {get; private set;}


        public BuildNewVM()
        {
            PageTitle = "Photon Server New Build";
        }

        public void Build()
        {
            if (string.IsNullOrEmpty(ProjectId))
                throw new ApplicationException("Project ID is undefined!");

            if (!PhotonServer.Instance.Projects.TryGet(ProjectId, out var project))
                throw new ApplicationException($"Project '{ProjectId}' not found!");

            ProjectFound = true;
            ProjectName = project.Description.Name;
            PreBuildCommand = project.Description.PreBuild;
            AssemblyFilename = project.Description.AssemblyFile;
            GitRefspec = "master";
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
            var _roles = TaskRoles.Split(new[] {',', ';'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).ToArray();

            if (!PhotonServer.Instance.Projects.TryGet(ProjectId, out var project))
                throw new ApplicationException($"Project '{ProjectId}' was not found!");

            var build = await project.StartNewBuild();

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

            //SessionId = session.SessionId;
            BuildNumber = session.Build.Number;
        }
    }
}
