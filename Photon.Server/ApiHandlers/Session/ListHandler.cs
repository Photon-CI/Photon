using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Photon.Library;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ApiHandlers.Session
{
    [HttpHandler("/api/sessions/active")]
    internal class ListHandler : HttpHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ListHandler));


        public override HttpHandlerResult Get()
        {
            return Response.Ok()
                .SetChunked(true)
                .SetContentType("text/event-stream")
                .SetContent(OnProcess);
        }

        private async Task OnProcess(ResponseBodyBuilder response, CancellationToken token)
        {
            using (var socket = new WebSocketHost(response.GetStream()))
            using (var watch = new ServerSessionWatch()) {
                socket.Send("open");

                var wRef = new WeakReference<WebSocketHost>(socket);

                watch.SessionChanged += (o, e) => {
                    if (wRef.TryGetTarget(out var _socket)) {
                        try {
                            _socket.Send("message", e.Data);
                        }
                        catch (Exception error) {
                            Log.Warn("Failed to send WebSocket message!", error);
                        }
                    }
                };

                watch.Initialize();


                // TODO: GET RID OF THIS!!!
                // This is an awful hack, and never stops running!
                // Need to read request and close both when closed.
                while (!token.IsCancellationRequested) {
                    await Task.Delay(200, token);
                }
            }
        }
    }
}
