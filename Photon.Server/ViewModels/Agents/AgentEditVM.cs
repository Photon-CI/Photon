using Photon.Framework.Extensions;
using Photon.Framework.Server;
using Photon.Library;
using Photon.Server.Internal;
using System;
using System.Collections.Specialized;

namespace Photon.Server.ViewModels.Agents
{
    internal class AgentEditVM : ViewModelBase
    {
        public string AgentId_Source {get; set;}
        public string AgentId {get; set;}
        public string AgentName {get; set;}
        public string AgentHost {get; set;}
        public string AgentPort {get; set;}
        public string[] AgentRoles {get; set;}
        public bool IsNew {get; set;}


        public void Restore(NameValueCollection form)
        {
            AgentId_Source = form.Get("AgentId_Source");
            AgentId = form.Get("AgentId");
            AgentName = form.Get("AgentName");
            AgentHost = form.Get("AgentHost");
            AgentPort = form.Get("AgentPort");

            AgentRoles = form.Get("AgentRolesJson")
                .Split(new []{';'}, StringSplitOptions.RemoveEmptyEntries);
        }

        public void Build()
        {
            IsNew = string.IsNullOrEmpty(AgentId);

            if (IsNew) {
                AgentId_Source = AgentId = Guid.NewGuid().ToString("D");
                AgentName = "New Agent";
                AgentHost = "localhost";
                AgentPort = "10930";
            }
            else if (PhotonServer.Instance.Agents.TryGet(AgentId, out var agent)) {
                AgentId_Source = AgentId = agent.Id;
                AgentName = agent.Name;
                AgentHost = agent.TcpHost;
                AgentPort = agent.TcpPort.ToString();
                AgentRoles = agent.Roles.ToArray();
            }
        }

        public void Save()
        {
            var agent = new ServerAgent {
                Id = AgentId,
                Name = AgentName,
                TcpHost = AgentHost,
                TcpPort = AgentPort.To<int>(),
            };

            agent.Roles.AddRange(AgentRoles);

            string prevId = null;
            if (!string.Equals(AgentId_Source, AgentId, StringComparison.Ordinal))
                prevId = AgentId_Source;

            PhotonServer.Instance.Agents.Save(agent, prevId);
        }
    }
}
