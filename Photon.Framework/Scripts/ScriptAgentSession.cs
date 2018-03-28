using Newtonsoft.Json;
using Photon.Framework.Extensions;
using Photon.Framework.Sessions;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Photon.Framework.Scripts
{
    public class ScriptAgentSession
    {
        private readonly AgentDefinition agentDefinition;
        public string SessionId {get; private set;}


        public ScriptAgentSession(AgentDefinition agentDefinition)
        {
            this.agentDefinition = agentDefinition;
        }

        public async Task BeginAsync()
        {
            var url = NetPath.Combine(agentDefinition.Address, "session/begin");
            
            var httpRequest = WebRequest.CreateHttp(url);
            httpRequest.Method = "POST";
            httpRequest.ContentLength = 0;

            using (var httpResponse = (HttpWebResponse)await httpRequest.GetResponseAsync()) {
                if (httpResponse.StatusCode != HttpStatusCode.OK)
                    throw new ApplicationException($"Agent returned status code [{(int)httpResponse.StatusCode}] {httpResponse.StatusDescription}");

                using (var responseStream = httpResponse.GetResponseStream()) {
                    var serializer = new JsonSerializer();
                    var response = serializer.Deserialize<SessionBeginResponse>(responseStream);

                    SessionId = response.SessionId;
                }
            }
        }

        public async Task ReleaseAsync()
        {
            // TODO: Locking on sessionId and isActive

            var url = NetPath.Combine(agentDefinition.Address, "session/release");
            
            var httpRequest = WebRequest.CreateHttp(url);
            httpRequest.Method = "POST";
            httpRequest.ContentLength = 0;

            using (var httpResponse = (HttpWebResponse)await httpRequest.GetResponseAsync()) {
                if (httpResponse.StatusCode != HttpStatusCode.OK)
                    throw new ApplicationException($"Agent returned status code [{(int)httpResponse.StatusCode}] {httpResponse.StatusDescription}");

                //...
            }
        }

        public async Task RunTask(string taskName)
        {
            throw new NotImplementedException();
        }
    }
}
