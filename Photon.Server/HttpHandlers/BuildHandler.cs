using log4net;
using Newtonsoft.Json;
using Photon.Framework.Extensions;
using Photon.Framework.Scripts;
using Photon.Library.Messages;
using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using System;
using System.IO;
using Photon.Framework.Messages;

namespace Photon.Server.HttpHandlers
{
    [HttpHandler("/build")]
    internal class BuildHandler : HttpHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BuildHandler));


        public override HttpHandlerResult Post()
        {
            var qProjectId = GetQuery("project");
            var qGitRefspec = GetQuery("refspec");
            var qScriptName = GetQuery("script");
            var qAssemblyFile = GetQuery("assembly");

            BuildStartInfo startInfo = null;
            if (HttpContext.Request.ContentLength64 > 0) {
                var serializer = new JsonSerializer();
                startInfo = serializer.Deserialize<BuildStartInfo>(HttpContext.Request.InputStream);
            }

            var _projectId = startInfo?.ProjectId ?? qProjectId;
            var _gitRefspec = startInfo?.GitRefspec ?? qGitRefspec;
            var _scriptName = startInfo?.ScriptName ?? qScriptName;
            var _assemblyFile = startInfo?.AssemblyFile ?? qAssemblyFile;

            //Log.Debug($"Running Build-Script '{_scriptName}' from Project '{_projectId}' @ '{_gitRefspec}'.");

            try {
                if (!PhotonServer.Instance.Projects.TryGet(_projectId, out var project))
                    return BadRequest().SetText($"Project '{_projectId}' was not found!");

                var projectData = PhotonServer.Instance.ProjectData.GetOrCreate(project.Id);
                var buildNumber = projectData.StartNewBuild();

                var context = new ServerBuildContext {
                    WorkDirectory = Configuration.WorkDirectory,
                    BuildNumber = buildNumber,
                    AssemblyFile = _assemblyFile,
                    Project = project,
                    ScriptName = _scriptName,
                    RefSpec = _gitRefspec,
                    Agents = PhotonServer.Instance.Definition.Agents.ToArray(),
                };

                var session = new ServerBuildSession(context);

                PhotonServer.Instance.Sessions.BeginSession(session);
                PhotonServer.Instance.Queue.Add(session);

                var response = new BuildSessionBeginResponse {
                    SessionId = session.SessionId,
                };

                var memStream = new MemoryStream();

                try {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(memStream, response, true);

                    return Ok()
                        .SetContentType("application/json")
                        .SetContent(memStream);
                }
                catch {
                    memStream.Dispose();
                    throw;
                }
            }
            catch (Exception error) {
                Log.Error($"Failed to run Build-Script '{_scriptName}' from Project '{_projectId}' @ '{_gitRefspec}'!", error);
                return Exception(error);
            }
        }
    }
}
