using Photon.Framework;
using Photon.Framework.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Photon.CLI.Internal.Http
{
    internal class HttpStatusCodeException : Exception
    {
        public HttpStatusCode HttpCode {get;}
        public string HttpDescription {get;}

        public HttpStatusCodeException(HttpStatusCode code, string description)
            : base($"Server Responded with [{(int)code}] {description}")
        {
            HttpCode = code;
            HttpDescription = description;
        }
    }

    internal class HttpClientEx : IDisposable
    {
        private static readonly HttpStatusCode[] SafeCodes = {
            HttpStatusCode.NotModified,
            HttpStatusCode.BadRequest,
        };

        public string Url {get; set;}
        public object Query {get; set;}
        public string Method {get; set;}
        public string ContentType {get; set;}
        public Stream Body {get; set;}
        public Func<Stream> BodyFunc {get; set;}
        
        public HttpWebRequest RequestBase {get; private set;}
        public HttpWebResponse ResponseBase {get; private set;}


        public void Dispose()
        {
            Body?.Dispose();
            ResponseBase?.Dispose();
        }

        public async Task Send()
        {
            RequestBase = await BuildRequest();
            ResponseBase = await GetResponse(RequestBase);
        }

        public T ParseJsonResponse<T>()
        {
            using (var responseStream = ResponseBase.GetResponseStream()) {
                if (responseStream == null) return default(T);

                return JsonSettings.Serializer.Deserialize<T>(responseStream);
            }
        }

        public async Task<string> GetResponseTextAsync()
        {
            using (var responseStream = ResponseBase.GetResponseStream()) {
                if (responseStream == null) return null;

                using (var reader = new StreamReader(responseStream)) {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        private async Task<HttpWebRequest> BuildRequest()
        {
            var _url = Url;

            if (Query != null)
                _url += NetPath.QueryString(Query);

            var request = WebRequest.CreateHttp(_url);
            request.Method = Method;
            request.KeepAlive = true;
            request.ContentType = ContentType;

            var requestBody = Body ?? BodyFunc?.Invoke();

            if (requestBody != null) {
                request.ContentLength = requestBody.Length - requestBody.Position;

                try {
                    using (var requestStream = request.GetRequestStream()) {
                        await requestBody.CopyToAsync(requestStream);
                    }
                }
                catch (WebException error) {
                    throw new HttpStatusCodeException(HttpStatusCode.NotFound, error.Message);
                }
            }
            else {
                request.ContentLength = 0;
            }

            return request;
        }

        private static async Task<HttpWebResponse> GetResponse(WebRequest request)
        {
            HttpWebResponse response = null;

            try {
                response = (HttpWebResponse) await request.GetResponseAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new HttpStatusCodeException(response.StatusCode, response.StatusDescription);

                return response;
            }
            catch (WebException error) when (error.Response is HttpWebResponse errorResponse) {
                if (SafeCodes.Any(x => x == errorResponse.StatusCode))
                    return errorResponse;

                response?.Dispose();
                throw new HttpStatusCodeException(errorResponse.StatusCode, errorResponse.StatusDescription);
            }
            catch {
                response?.Dispose();
                throw;
            }
        }

        public static HttpClientEx Get(string url, object query = null)
        {
            return new HttpClientEx {
                Url = url,
                Method = "GET",
                Query = query,
            };
        }

        public static HttpClientEx Post(string url, object query = null)
        {
            return new HttpClientEx {
                Url = url,
                Method = "POST",
                Query = query,
            };
        }
    }
}
