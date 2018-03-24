﻿using PiServerLite.Http.Handlers;

namespace Photon.Server.Handlers
{
    [HttpHandler("/")]
    [HttpHandler("/index")]
    internal class IndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            return View("Index.html");
        }
    }
}
