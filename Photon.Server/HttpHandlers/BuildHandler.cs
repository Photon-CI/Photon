using log4net;
using Newtonsoft.Json;
using Photon.Framework.Extensions;
using Photon.Library.HttpMessages;
using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using System;
using System.IO;

namespace Photon.Server.HttpHandlers
{
    [HttpHandler("/build")]
    internal class BuildHandler : HttpHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BuildHandler));


        public override HttpHandlerResult Post()
        {
            var qGitRefspec = GetQuery("refspec");

            BuildStartInfo startInfo = null;
            if (HttpContext.Request.ContentLength64 > 0) {
                var serializer = new JsonSerializer();
                startInfo = serializer.Deserialize<BuildStartInfo>(HttpContext.Request.InputStream);
            }

            if (startInfo == null)
                return BadRequest().SetText("No json request was found!");

            var _gitRefspec = qGitRefspec ?? startInfo.GitRefspec;

            try {
                if (!PhotonServer.Instance.Projects.TryGet(startInfo.ProjectId, out var project))
                    return BadRequest().SetText($"Project '{startInfo.ProjectId}' was not found!");

                var projectData = PhotonServer.Instance.ProjectData.GetOrCreate(project.Id);
                var buildNumber = projectData.StartNewBuild();

                var session = new ServerBuildSession {
                    Project = project,
                    AssemblyFile = startInfo.AssemblyFile,
                    PreBuild = startInfo.PreBuild,
                    TaskName = startInfo.TaskName,
                    GitRefspec = _gitRefspec,
                    BuildNumber = buildNumber,
                    Roles = startInfo.Roles,
                };

                PhotonServer.Instance.Sessions.BeginSession(session);
                PhotonServer.Instance.Queue.Add(session);

                var response = new HttpBuildStartResponse {
                    SessionId = session.SessionId,
                };

                var memStream = new MemoryStream();

                try {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(memStream, response, true);
                }
                catch {
                    memStream.Dispose();
                    throw;
                }

                return Ok()
                    .SetContentType("application/json")
                    .SetContent(memStream);
            }
            catch (Exception error) {
                Log.Error($"Failed to run Build-Task '{startInfo.TaskName}' from Project '{startInfo.ProjectId}' @ '{_gitRefspec}'!", error);
                return Exception(error);
            }
        }
    }
}
