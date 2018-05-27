using log4net;
using Photon.Framework.Projects;
using Photon.Library;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Photon.Server.Internal.Projects
{
    internal class ProjectManager2
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProjectManager2));

        private readonly ConcurrentDictionary<string, ServerProject> projectsCollection;

        public IEnumerable<ServerProject> All => projectsCollection.Values;


        public ProjectManager2()
        {
            projectsCollection = new ConcurrentDictionary<string, ServerProject>();
        }

        public bool TryGet(string projectId, out ServerProject project)
        {
            return projectsCollection.TryGetValue(projectId, out project);
        }

        public bool TryGetDescription(string projectId, out Project projectDescription)
        {
            if (projectsCollection.TryGetValue(projectId, out var project)) {
                projectDescription = project.Description;
                return true;
            }

            projectDescription = null;
            return false;
        }

        public async Task Load(CancellationToken token = default(CancellationToken))
        {
            if (!Directory.Exists(Configuration.ProjectsDirectory)) return;

            var blockOptions = new ExecutionDataflowBlockOptions {
                MaxDegreeOfParallelism = Configuration.Parallelism,
                CancellationToken = token,
            };

            var block = new ActionBlock<string>(projectIndexFile => {
                var project = new ServerProject {
                    ContentPath = Path.GetDirectoryName(projectIndexFile),
                };

                project.Load();

                var projectId = project.Description?.Id ?? string.Empty;
                projectsCollection[projectId] = project;
            }, blockOptions);

            var root = Configuration.ProjectsDirectory;
            foreach (var path in Directory.EnumerateDirectories(root, "*", SearchOption.TopDirectoryOnly)) {
                var projectIndexFile = Path.Combine(path, "project.json");
                if (!File.Exists(projectIndexFile)) {
                    Log.Warn($"Missing project.json in '{path}'.");
                    continue;
                }

                block.Post(projectIndexFile);
            }

            block.Complete();
            await block.Completion;
        }

        public ServerProject New(string id)
        {
            if (projectsCollection.ContainsKey(id))
                throw new ApplicationException($"Project '{id}' already exists!");

            var path = GetProjectPath(id);

            var project = new ServerProject {
                ContentPath = path,
                Description = new Project {
                    Id = id,
                }
            };

            projectsCollection[id] = project;
            project.Save();

            return project;
        }

        public bool Remove(string id)
        {
            if (projectsCollection.TryRemove(id, out var project)) {
                if (Directory.Exists(project.ContentPath))
                    FileUtils.DestoryDirectory(project.ContentPath);

                return true;
            }

            return false;
        }

        public bool Rename(string prevId, string newId)
        {
            if (string.Equals(prevId, newId)) return true;

            if (!projectsCollection.TryGetValue(prevId, out var project)) return false;

            var newPath = GetProjectPath(newId);

            if (Directory.Exists(project.ContentPath)) {
                try {
                    Directory.Move(project.ContentPath, newPath);
                }
                catch (Exception error) {
                    Log.Error($"Failed to move project directory '{prevId}' to '{newId}'!", error);
                    return false;
                }
            }
            else {
                Log.Warn($"No directory found when renaming project '{prevId}' to '{newId}'.");
            }

            project.Description.Id = newId;
            project.ContentPath = newPath;

            projectsCollection.TryRemove(prevId, out _);
            projectsCollection[newId] = project;

            return true;
        }

        private static string GetProjectPath(string id)
        {
            return Path.Combine(Configuration.ProjectsDirectory, id);
        }
    }
}
