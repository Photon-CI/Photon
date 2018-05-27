using Photon.Agent.Internal;
using PiServerLite.Http.Handlers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Agent.HttpHandlers.Api.Session
{
    [HttpHandler("api/session/output-stream")]
    internal class OutputStreamHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var sessionId = GetQuery("id");

            if (string.IsNullOrEmpty(sessionId))
                return Response.BadRequest().SetText("'id' is undefined!");

            try {
                if (!PhotonAgent.Instance.Sessions.TryGet(sessionId, out var session))
                    return Response.BadRequest().SetText($"Session '{sessionId}' not found!");

                return Response.Ok()
                    .SetChunked(true)
                    .SetContentType("text/octet-stream")
                    .SetContent(async (r, t) => {
                        using (var stream = r.GetStream())
                        using (var writer = new StreamWriter(stream)) {
                            writer.AutoFlush = true;
                            var pos = 0;
                            while (!t.IsCancellationRequested) {
                                if (session.Output.Length > pos) {
                                    var text = session.Output.Writer
                                        .GetString().Substring(pos);

                                    await writer.WriteAsync(text);

                                    pos = session.Output.Length;
                                }
                                else if (session.IsReleased) {
                                    break;
                                }

                                await Task.Delay(100, t);
                            }

                            t.ThrowIfCancellationRequested();
                        }
                    });
            }
            catch (Exception error) {
                return Response.Exception(error);
            }
        }
    }
}
