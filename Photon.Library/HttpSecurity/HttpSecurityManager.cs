using Photon.Framework.Pooling;
using PiServerLite.Http;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;
using System.Net;
using System.Text;

namespace Photon.Library.HttpSecurity
{
    public class HttpSecurityManager : ISecurityManager
    {
        //private const string CookieName = "PHOTON.SERVER.AUTH";

        private readonly ReferencePool<HttpUserContext> userTokens;
        
        public string CookieName {get; set;}
        public IAuthorize Authorization {get; set;}
        public bool Restricted {get; set;}


        public HttpSecurityManager()
        {
            CookieName = "PHOTON.AUTH";
            userTokens = new ReferencePool<HttpUserContext>();
        }

        public bool GetUserContext(HttpListenerRequest request, out HttpUserContext userContext)
        {
            if (GetCookieUserContext(request, out userContext)) {
                userContext.Restart();
                return true;
            }

            if (GetBasicAuthUserContext(request, out userContext)) {
                userContext.Restart();
                return true;
            }

            userContext = null;
            return false;
        }

        public bool Authorize(HttpListenerRequest request)
        {
            if (!Restricted) return true;

            return GetUserContext(request, out _);
        }

        public bool Authorize(HttpListenerResponse response, HttpUserCredentials user)
        {
            if (!Restricted) return true;

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

        private bool GetCookieUserContext(HttpListenerRequest request, out HttpUserContext userContext)
        {
            var authCookie = request.Cookies[CookieName];
            var token = authCookie?.Value;

            if (!string.IsNullOrEmpty(token) && userTokens.TryGet(token, out userContext)) {
                return true;
            }

            userContext = null;
            return false;
        }

        private bool GetBasicAuthUserContext(HttpListenerRequest request, out HttpUserContext userContext)
        {
            var authHeader = request.Headers.Get("Authorization");

            if (authHeader != null) {
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
                        return userContext != null;
                    }
                }
            }

            userContext = null;
            return false;
        }
    }
}
