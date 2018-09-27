using Photon.Framework.Server;
using System;
using System.Collections.Concurrent;

namespace Photon.Server.Internal.AgentConnections
{
    internal class AgentConnectionManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, AgentConnection> connectionList;


        public AgentConnectionManager()
        {
            connectionList = new ConcurrentDictionary<string, AgentConnection>();
        }

        public void Dispose()
        {
            foreach (var connection in connectionList.Values)
                connection.Dispose();

            connectionList.Clear();
        }

        public bool Get(string connectionId, out AgentConnection connection)
        {
            return connectionList.TryGetValue(connectionId, out connection);
        }

        public AgentConnection Create(ServerAgent agent)
        {
            var connection = new AgentConnection(agent);
            connection.Released += Connection_OnReleased;

            connectionList[connection.ConnectionId] = connection;
            return connection;
        }

        private void Connection_OnReleased(object sender, AgentConnectionReleaseEventArgs e)
        {
            connectionList.TryRemove(e.ConnectionId, out _);
        }
    }
}
