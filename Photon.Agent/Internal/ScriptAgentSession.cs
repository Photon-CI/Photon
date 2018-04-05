using System;
using System.Reflection;
using System.Threading.Tasks;
using Photon.Communication;
using Photon.Framework;
using Photon.Library.Messages;

namespace Photon.Agent.Internal
{
    public class ScriptAgentSession : IDisposable
    {
        private readonly AgentDefinition agentDefinition;
        private readonly MessageProcessor messageProcessor;
        private readonly MessageClient messageClient;
        public string SessionId {get; private set;}


        public ScriptAgentSession(AgentDefinition agentDefinition)
        {
            this.agentDefinition = agentDefinition;

            messageProcessor = new MessageProcessor();
            messageProcessor.Scan(Assembly.GetExecutingAssembly());

            messageClient = new MessageClient(messageProcessor);
        }

        public void Dispose()
        {
            messageClient?.Dispose();
        }

        public async Task BeginAsync()
        {
            messageProcessor.Start();

            await messageClient.ConnectAsync("localhost", 10933);

            var message = new BuildSessionBeginRequest();

            var handle = messageClient.Send(message);
            var response = await handle.GetResponseAsync<BuildSessionBeginResponse>();

            SessionId = response?.SessionId;
            if (string.IsNullOrEmpty(SessionId))
                throw new ApplicationException("Failed to begin agent session!");

            //Log.Debug($"Started session '{response.SessionId}' successfully.");

            //var url = NetPath.Combine(agentDefinition.Url, "session/begin");
            
            //var httpRequest = WebRequest.CreateHttp(url);
            //httpRequest.Method = "POST";
            //httpRequest.ContentLength = 0;

            //using (var httpResponse = (HttpWebResponse)await httpRequest.GetResponseAsync()) {
            //    if (httpResponse.StatusCode != HttpStatusCode.OK)
            //        throw new ApplicationException($"Agent returned status code [{(int)httpResponse.StatusCode}] {httpResponse.StatusDescription}");

            //    using (var responseStream = httpResponse.GetResponseStream()) {
            //        var serializer = new JsonSerializer();
            //        var response = serializer.Deserialize<SessionBeginResponse>(responseStream);

            //        SessionId = response.SessionId;
            //    }
            //}
        }

        public async Task ReleaseAsync()
        {
            // TODO: Locking on sessionId and isActive

            var message = new BuildSessionReleaseRequest {
                SessionId = SessionId,
            };

            var handle = messageClient.Send(message);
            var response = await handle.GetResponseAsync<BuildSessionReleaseResponse>();

            if (!(response?.Successful ?? false))
                throw new ApplicationException("Failed to release agent session!");

            await messageClient.DisconnectAsync();

            await messageProcessor.StopAsync();

            //var url = NetPath.Combine(agentDefinition.Url, "session/release")
            //    +NetPath.QueryString(new {session = SessionId});
            
            //var httpRequest = WebRequest.CreateHttp(url);
            //httpRequest.Method = "POST";
            //httpRequest.ContentLength = 0;

            //using (var httpResponse = (HttpWebResponse)await httpRequest.GetResponseAsync()) {
            //    if (httpResponse.StatusCode != HttpStatusCode.OK)
            //        throw new ApplicationException($"Agent returned status code [{(int)httpResponse.StatusCode}] {httpResponse.StatusDescription}");

            //    ...
            //}
        }

        public async Task RunTask(string taskName)
        {
            var taskId = await BeginTask(taskName);

            // TODO: Poll HTTP for task completion
            //    until TCP is implemented and can
            //    post completion event.
            throw new NotImplementedException();
        }

        private async Task<string> BeginTask(string taskName)
        {
            var message = new TaskBeginRequest {
                SessionId = SessionId,
                TaskName = taskName,
            };

            var handle = messageClient.Send(message);
            var response = await handle.GetResponseAsync<TaskBeginResponse>();

            var taskId = response?.TaskId;
            if (string.IsNullOrEmpty(taskId))
                throw new ApplicationException("Failed to begin agent task!");

            return taskId;

            //var url = NetPath.Combine(agentDefinition.Url, "task/begin")
            //    +NetPath.QueryString(new {
            //        session = SessionId,
            //        task = taskName,
            //    });
            
            //var httpRequest = WebRequest.CreateHttp(url);
            //httpRequest.Method = "POST";
            //httpRequest.ContentLength = 0;

            //using (var httpResponse = (HttpWebResponse)await httpRequest.GetResponseAsync()) {
            //    if (httpResponse.StatusCode != HttpStatusCode.OK)
            //        throw new ApplicationException($"Agent returned status code [{(int)httpResponse.StatusCode}] {httpResponse.StatusDescription}");

            //    using (var responseStream = httpResponse.GetResponseStream()) {
            //        var serializer = new JsonSerializer();
            //        var response = serializer.Deserialize<SessionBeginResponse>(responseStream);

            //        SessionId = response.SessionId;
            //    }
            //}
        }
    }
}
