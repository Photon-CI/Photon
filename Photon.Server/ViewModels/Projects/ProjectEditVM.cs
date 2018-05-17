using Photon.Framework.Projects;
using Photon.Library;
using Photon.Server.Internal;
using System;
using System.Collections.Specialized;

namespace Photon.Server.ViewModels.Projects
{
    internal class ProjectEditVM : ViewModelBase
    {
        public string ProjectId_Source {get; set;}
        public string ProjectId {get; set;}
        public string ProjectName {get; set;}
        public string ProjectDescription {get; set;}
        public bool IsNew {get; set;}


        public void Restore(NameValueCollection form)
        {
            ProjectId_Source = form.Get(nameof(ProjectId_Source));
            ProjectId = form.Get(nameof(ProjectId));
            ProjectName = form.Get(nameof(ProjectName));
            ProjectDescription = form.Get(nameof(ProjectDescription));
        }

        public void Build()
        {
            IsNew = string.IsNullOrEmpty(ProjectId);

            if (IsNew) {
                ProjectId_Source = ProjectId = Guid.NewGuid().ToString("D");
                ProjectName = "New Project";
                ProjectDescription = "";
            }
            else if (PhotonServer.Instance.Projects.TryGet(ProjectId, out var project)) {
                ProjectId_Source = ProjectId = project.Id;
                ProjectName = project.Name;
                ProjectDescription = project.Description;
            }
        }

        public void Save()
        {
            if (!PhotonServer.Instance.Projects.TryGet(ProjectId, out var project))
                project = new Project {Id = ProjectId};

            project.Name = ProjectName;
            project.Description = ProjectDescription;

            string prevId = null;
            if (!string.Equals(ProjectId_Source, ProjectId, StringComparison.Ordinal))
                prevId = ProjectId_Source;

            PhotonServer.Instance.Projects.Save(project, prevId);
        }
    }
}
