using Photon.CLI.Internal;
using Photon.CLI.Internal.Http;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Photon.CLI.Actions
{
    internal class ActionBase
    {
        private bool WasPromptShown;

        public string Username {get; set;}
        public string Password {get; set;}
        public bool Passive {get; set;}

        public bool HasCredentials => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);


        protected void HttpAuth(Action httpAction)
        {
            while (true) {
                try {
                    httpAction();
                    break;
                }
                catch (HttpUnauthorizedException) {
                    if (!HasCredentials && !WasPromptShown && !Passive) {
                        ShowCredentialsPrompt();
                        continue;
                    }
                    
                    throw new ApplicationException("Request Unauthorized!");
                }
            }
        }

        protected async Task HttpAuthAsync(Func<Task> httpAction)
        {
            while (true) {
                try {
                    await httpAction();
                    break;
                }
                catch (HttpUnauthorizedException) {
                    if (!HasCredentials && !WasPromptShown && !Passive) {
                        ShowCredentialsPrompt();
                        continue;
                    }
                    
                    throw new ApplicationException("Request Unauthorized!");
                }
            }
        }

        protected async Task WebClient(PhotonServerDefinition server, Func<WebClient, Task> clientTask)
        {
            var client = WebClientFactory.Create(server, Username, Password);

            try {
                await clientTask(client);
            }
            catch (WebException error) {
                if (error.Response is HttpWebResponse httpResponse) {
                    if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                        throw new ApplicationException($"Photon-Server instance '{server.Name}' not found!");

                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                        throw new HttpUnauthorizedException();
                }

                throw;
            }
            finally {
                client.Dispose();
            }
        }

        protected async Task<T> WebClient<T>(PhotonServerDefinition server, Func<WebClient, Task<T>> clientTask)
        {
            var client = WebClientFactory.Create(server, Username, Password);

            try {
                return await clientTask(client);
            }
            catch (WebException error) {
                if (error.Response is HttpWebResponse httpResponse) {
                    if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                        throw new ApplicationException($"Photon-Server instance '{server.Name}' not found!");

                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                        throw new HttpUnauthorizedException();

                    if (httpResponse.StatusCode == HttpStatusCode.BadRequest) {
                        string text = null;
                        try {
                            using (var stream = httpResponse.GetResponseStream())
                            using (var reader = new StreamReader(stream)) {
                                text = await reader.ReadToEndAsync();
                            }
                        }
                        catch {}

                        throw new ApplicationException($"Bad Request! {text}");
                    }
                }

                throw;
            }
            finally {
                client.Dispose();
            }
        }

        protected async Task WebClientEx(PhotonServerDefinition server, Action<HttpClientEx> requestAction, Action<HttpClientEx> responseAction)
        {
            HttpClientEx client = null;

            try {
                client = new HttpClientEx {
                    Method = "GET",
                    Username = Username,
                    Password = Password,
                };

                requestAction?.Invoke(client);

                await client.Send();

                //if (client.ResponseBase.StatusCode == HttpStatusCode.Unauthorized)
                //    throw new ApplicationException("Access not authorized!");

                if (client.ResponseBase.StatusCode == HttpStatusCode.BadRequest) {
                    var text = await client.GetResponseTextAsync();
                    throw new ApplicationException($"Bad Update Request! {text}");
                }

                responseAction?.Invoke(client);
            }
            catch (HttpStatusCodeException error) {
                if (error.HttpCode == HttpStatusCode.NotFound)
                    throw new ApplicationException($"Photon-Server instance '{server.Name}' not found!");

                if (error.HttpCode == HttpStatusCode.Unauthorized)
                    throw new ApplicationException("Access not authorized!");

                throw;
            }
            finally {
                client?.Dispose();
            }
        }

        protected async Task<T> WebClientEx<T>(PhotonServerDefinition server, Action<HttpClientEx> requestAction, Func<HttpClientEx, T> responseAction)
        {
            HttpClientEx client = null;

            try {
                client = new HttpClientEx {
                    Method = "GET",
                    Username = Username,
                    Password = Password,
                };

                requestAction?.Invoke(client);

                await client.Send();

                //if (client.ResponseBase.StatusCode == HttpStatusCode.Unauthorized)
                //    throw new ApplicationException("Access not authorized!");

                if (client.ResponseBase.StatusCode == HttpStatusCode.BadRequest) {
                    var text = await client.GetResponseTextAsync();
                    throw new ApplicationException($"Bad Update Request! {text}");
                }

                if (responseAction != null)
                    return responseAction.Invoke(client);

                return default(T);
            }
            catch (HttpStatusCodeException error) {
                if (error.HttpCode == HttpStatusCode.NotFound)
                    throw new ApplicationException($"Photon-Server instance '{server.Name}' not found!");

                if (error.HttpCode == HttpStatusCode.Unauthorized)
                    throw new HttpUnauthorizedException();

                throw;
            }
            finally {
                client?.Dispose();
            }
        }

        private void ShowCredentialsPrompt()
        {
            WasPromptShown = true;

            ConsoleEx.Out
                .WriteLine("Request Unauthorized!", ConsoleColor.DarkYellow)
                .Write("Username: ", ConsoleColor.DarkCyan)
                .SetForegroundColor(ConsoleColor.Cyan);

            Username = Console.ReadLine();

            ConsoleEx.Out
                .Write("Password: ", ConsoleColor.DarkCyan)
                .SetForegroundColor(ConsoleColor.Cyan);

            Password = Console.ReadLine();

            ConsoleEx.Out.WriteLine();
        }
    }
}
