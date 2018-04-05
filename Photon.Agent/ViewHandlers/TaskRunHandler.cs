using System;
using System.Threading.Tasks;
using log4net;
using Photon.Agent.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewHandlers
{
    [HttpHandler("/task/begin")]
    internal class TaskRunHandler : HttpHandlerAsync
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TaskRunHandler));


        public override async Task<HttpHandlerResult> PostAsync()
        {
            var sessionId = GetQuery("session");
            var taskName = GetQuery("task");

            if (string.IsNullOrEmpty(sessionId))
                return BadRequest().SetText("'session' is undefined!");

            if (string.IsNullOrEmpty(taskName))
                return BadRequest().SetText("'task' is undefined!");

            if (!PhotonAgent.Instance.Sessions.TryGetSession(sessionId, out var session))
                return BadRequest().SetText($"Session '{sessionId}' not found!");

            try {
                // TODO: Begin the task, do not wait!
                //   Needs a context object

                throw new NotImplementedException();
                //await session.RunTask(taskName);

                return Ok().SetText("Ok");
                    //.SetContentType("application/json")
                    //.SetContent(memStream);
            }
            catch (Exception error) {
                Log.Error($"Failed to begin session!", error);
                return Exception(error);
            }
        }
    }
}
