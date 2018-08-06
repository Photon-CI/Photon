﻿using Photon.Agent.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewHandlers
{
    [HttpHandler("/logout")]
    internal class LogoutHandler : HttpHandler
    {
        public override HttpHandlerResult Get() => Run();
        public override HttpHandlerResult Post() => Run();

        private HttpHandlerResult Run()
        {
            var serverSecurity = (AgentHttpSecurity) Context.SecurityMgr;
            serverSecurity.SignOut(HttpContext);

            return Response.Redirect("/login");
        }
    }
}