using Photon.Server.Internal;
using Photon.Server.Internal.Projects;
using System;
using System.Collections.Specialized;

namespace Photon.Server.ViewModels.Project
{
    internal class ProjectEditVM : ServerViewModel
    {
        public string ProjectId_Source {get; set;}
        public string ProjectId {get; set;}
        public string ProjectName {get; set;}
        public string ProjectDescription {get; set;}

        public bool IsNew => string.IsNullOrEmpty(ProjectId_Source);


        public void Restore(NameValueCollection form)
        {
            ProjectId_Source = form.Get(nameof(ProjectId_Source));
            ProjectId = form.Get(nameof(ProjectId));
            ProjectName = form.Get(nameof(ProjectName));
            ProjectDescription = form.Get(nameof(ProjectDescription));
        }

        public void Build()
        {
            if (string.IsNullOrEmpty(ProjectId)) {
                ProjectId_Source = null;
                ProjectId = Guid.NewGuid().ToString("D");
                ProjectName = "New Project";
                ProjectDescription = "";
            }
            else if (PhotonServer.Instance.Projects.TryGetDescription(ProjectId, out var project)) {
                ProjectId_Source = ProjectId = project.Id;
                ProjectName = project.Name;
                ProjectDescription = project.Description;
            }
        }

        public void Save()
        {
            ServerProject project;

            if (string.IsNullOrEmpty(ProjectId_Source)) {
                project = PhotonServer.Instance.Projects.New(ProjectId);
            }
            else {
                if (!string.Equals(ProjectId_Source, ProjectId, StringComparison.OrdinalIgnoreCase)) {
                    if (!PhotonServer.Instance.Projects.Rename(ProjectId_Source, ProjectId))
                        throw new ApplicationException($"Failed to rename project '{ProjectId}'!");
                }

                if (!PhotonServer.Instance.Projects.TryGet(ProjectId, out project))
                    throw new ApplicationException($"Project '{ProjectId}' not found!");
            }

            project.Description.Name = ProjectName;
            project.Description.Description = ProjectDescription;

            project.Save();
        }
    }
}
