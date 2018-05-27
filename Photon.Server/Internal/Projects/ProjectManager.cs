using log4net;
using Newtonsoft.Json.Linq;
using Photon.Framework.Projects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Photon.Server.Internal.Projects
{
    internal class ProjectManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProjectManager));

        private readonly JsonDynamicDocument projectsDocument;
        private readonly ConcurrentDictionary<string, Project> projectsCollection;

        public IEnumerable<Project> All => projectsCollection.Values;


        public ProjectManager()
        {
            projectsDocument = new JsonDynamicDocument {
                Filename = Configuration.ProjectsFile,
            };

            projectsCollection = new ConcurrentDictionary<string, Project>();
        }

        public bool TryGet(string projectId, out Project project)
        {
            return projectsCollection.TryGetValue(projectId, out project);
        }

        public void Load()
        {
            projectsDocument.Load(Document_OnLoad);
        }

        public void Remove(string id)
        {
            projectsCollection.TryRemove(id, out var _);

            projectsDocument.Remove(d => Document_OnRemove(d, id));
        }

        public void Save(Project project, string prevId = null)
        {
            if (prevId != null) {
                projectsCollection.TryRemove(prevId, out var _);
            }

            projectsCollection.AddOrUpdate(project.Id, project, (k, a) => {
                a.Id = project.Id;
                a.Name = project.Name;
                a.Description = project.Description;
                return a;
            });

            projectsDocument.Update(d => Document_OnUpdate(d, project, prevId));
        }

        //public static object GetSource(dynamic source, string type)
        //{
        //    switch (type.ToLower()) {
        //        case "github":
        //            return (ProjectGithubSource)source.ToObject<ProjectGithubSource>();
        //        case "fs":
        //            return (ProjectFileSystemSource)source?.ToObject<ProjectFileSystemSource>();
        //        default:
        //            throw new ApplicationException($"Unknown source type '{type}'!");
        //    }
        //}

        private void Document_OnLoad(JToken document)
        {
            if (!(document?.Root is JArray projectArray)) return;

            projectsCollection.Clear();
            foreach (var projectDef in projectArray) {
                var project = projectDef.ToObject<Project>();

                if (string.IsNullOrEmpty(project?.Id)) {
                    Log.Warn($"Unable to load project definition '{project?.Name}'! Project 'id' is undefined.");
                    continue;
                }

                projectsCollection[project.Id] = project;
            }
        }

        private void Document_OnUpdate(JToken document, Project project, string prevId)
        {
            if (!(document.Root is JArray projectArray)) {
                projectArray = new JArray();
                document.Replace(projectArray);
                var token = JObject.FromObject(project);
                projectArray.Add(token);
            }
            else {
                var found = false;
                foreach (var projectDef in projectArray) {
                    var _project = projectDef.ToObject<Project>();

                    var _id = prevId ?? project.Id;
                    if (!string.Equals(_project.Id, _id)) continue;

                    var projectToken = JToken.FromObject(project);
                    projectDef.Replace(projectToken);
                    found = true;
                    break;
                }

                if (!found) {
                    var projectToken = JToken.FromObject(project);
                    projectArray.Add(projectToken);
                }
            }
        }

        private bool Document_OnRemove(JToken document, string id)
        {
            if (!(document.Root is JArray projectArray))
                return false;

            foreach (var projectDef in projectArray) {
                var _project = projectDef.ToObject<Project>();
                if (!string.Equals(_project.Id, id)) continue;

                projectDef.Remove();
                return true;
            }

            return false;
        }
    }
}
