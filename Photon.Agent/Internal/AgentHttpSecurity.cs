using Photon.Library.HttpSecurity;
using PiServerLite.Http;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System.Net;

namespace Photon.Agent.Internal
{
    internal class AgentHttpSecurity : ISecurityManager
    {
        public bool Authorize(HttpListenerRequest request)
        {
            // TODO: Verify a request with existing credentials

            return true;
        }

        public bool Authenticate(HttpListenerResponse response, HttpUserCredentials user)
        {
            // TODO: Verify new credentials

            return true;
        }

        public void SignOut(HttpListenerContext httpContext)
        {
            // TODO: Remove existing credentials
        }

        public HttpHandlerResult OnUnauthorized(HttpListenerContext httpContext, HttpReceiverContext context)
        {
            var returnUrl = httpContext.Request.RawUrl;
            return HttpHandlerResult.Redirect(context, "/login", new {returnUrl});
        }
    }
}
