using Photon.Framework.Pooling;
using Photon.Library.HttpSecurity;
using PiServerLite.Http;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;
using System.Net;
using System.Text;

namespace Photon.Agent.Internal
{
    internal class AgentHttpSecurity : ISecurityManager
    {
        private const string CookieName = "PHOTON.AGENT.AUTH";

        private readonly ReferencePool<HttpUserContext> userTokens;
        
        public IAuthorize Authorization {get; set;}


        public AgentHttpSecurity()
        {
            userTokens = new ReferencePool<HttpUserContext>();
            //userTokens.PruneInterval = ?;
        }

        public bool Authorize(HttpListenerRequest request)
        {
            // Authorization Cookie
            var authCookie = request.Cookies[CookieName];
            var token = authCookie?.Value;

            if (!string.IsNullOrEmpty(token) && userTokens.TryGet(token, out var userContext)) {
                userContext.Restart();
                return true;
            }

            // Authorization Header
            var authHeader = request.Headers.Get("Authorization");
            if (authHeader != null) {
                // Basic Authorization Header
                if (authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase)) {
                    var encodedAuth = authHeader.Substring(6).Trim();
                    var authBytes = Convert.FromBase64String(encodedAuth);
                    var authKey = Encoding.UTF8.GetString(authBytes);

                    var i = authKey.IndexOf(':');
                    if (i >= 0) {
                        var userCreds = new HttpUserCredentials {
                            Username = authKey.Substring(0, i),
                            Password = authKey.Substring(i + 1),
                        };

                        userContext = Authorization.Authorize(userCreds);

                        if (userContext != null) {
                            userContext.Restart();
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool Authorize(HttpListenerResponse response, HttpUserCredentials user)
        {
            var _user = Authorization.Authorize(user);
            if (_user == null) return false;

            _user.Lifespan = TimeSpan.FromMinutes(60);
            _user.Restart();

            userTokens.Add(_user);

            var cookie = new Cookie(CookieName, _user.SessionId) {
                Expires = DateTime.Now.AddYears(1),
            };

            response.SetCookie(cookie);

            return true;
        }

        public void SignOut(HttpListenerContext httpContext)
        {
            var authCookie = httpContext.Request.Cookies[CookieName];

            if (!string.IsNullOrEmpty(authCookie?.Value))
                userTokens.Remove(authCookie.Value);

            httpContext.Response.SetCookie(new Cookie {
                Name = CookieName,
                Expires = DateTime.Now.AddYears(-1),
            });
        }

        public HttpHandlerResult OnUnauthorized(HttpListenerContext httpContext, HttpReceiverContext context)
        {
            var returnUrl = httpContext.Request.RawUrl;
            return HttpHandlerResult.Redirect(context, "/login", new {returnUrl});
        }
    }
}
