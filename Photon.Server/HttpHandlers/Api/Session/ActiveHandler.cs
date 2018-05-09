using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.HttpHandlers.Api.Session
{
    [HttpHandler("/api/sessions/active")]
    internal class ActiveHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            return Ok()
                .SetChunked(true)
                .SetContentType("text/event-stream")
                .SetContent(OnProcess);
        }

        private async Task OnProcess(Stream stream, CancellationToken token)
        {
            using (var socket = new WebSocketHost(stream))
            using (var watch = new ServerSessionWatch()) {
                socket.Send("open");

                var wRef = new WeakReference<WebSocketHost>(socket);

                watch.SessionChanged += (o, e) => {
                    if (wRef.TryGetTarget(out var _socket)) {
                        try {
                            _socket.Send("message", e.Data);
                        }
                        catch {}
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
