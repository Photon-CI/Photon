using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Server.HttpHandlers.Api.Session
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
                if (!PhotonServer.Instance.Sessions.TryGet(sessionId, out var session))
                    return Response.BadRequest().SetText($"Session '{sessionId}' not found!");

                return Response.Ok()
                    .SetChunked(true)
                    .SetContentType("application/octet-stream")
                    .SetContent(async (r, t) => {
                        using (var stream = r.GetStream())
                        using (var writer = new StreamWriter(stream)) {
                            //writer.AutoFlush = false;
                            var pos = 0;
                            while (!t.IsCancellationRequested) {
                                

                                if (session.Output.Length > pos) {
                                    var text = session.Output.ToString().Substring(pos);
                                    await writer.WriteAsync(text);

                                    pos = session.Output.Length;
                                }
                                else if (session.IsComplete) {
                                    break;
                                }

                                await Task.Delay(100, t);
                            }

                            t.ThrowIfCancellationRequested();
                            await writer.FlushAsync();
                        }
                    });
            }
            catch (Exception error) {
                return Response.Exception(error);
            }
        }
    }
}
