﻿using Photon.Server.Internal;
using Photon.Server.Internal.HealthChecks;
using Photon.Server.Internal.Security;
using PiServerLite.Http.Handlers;
using System.Linq;

namespace Photon.Server.ViewModels.Agent
{
    internal class AgentIndexVM : ServerViewModel
    {
        public AgentRow[] Agents {get; set;}
        public bool UserCanEdit {get; set;}


        public AgentIndexVM(IHttpHandler handler) : base(handler) {}

        protected override void OnBuild()
        {
            base.OnBuild();

            UserCanEdit = !Master.IsSecured || PhotonServer.Instance.UserMgr.UserHasRole(Master.UserContext.UserId, GroupRole.AgentEdit);

            Agents = PhotonServer.Instance.Agents.All
                .Select(x => {
                    var status = PhotonServer.Instance.HealthChecks.GetStatus(x.Id);
                    var statusClass = GetStatusClass(status?.Status ?? AgentStatus.Pending);
                    
                    return new AgentRow {
                        Id = x.Id,
                        Name = x.Name,
                        Version = status?.AgentVersion,
                        Roles = x.Roles.OrderBy(y => y).ToArray(),
                        Class = statusClass,
                    };
                })
                .OrderBy(x => x.Name).ToArray();
        }

        private string GetStatusClass(AgentStatus status)
        {
            switch (status) {
                case AgentStatus.Ok: return "fas fa-check-circle text-success";
                case AgentStatus.Disconnected: return "fas fa-unlink text-danger";
                case AgentStatus.Warning: return "fas fa-exclamation-triangle text-warning";
                case AgentStatus.Error: return "fas fa-exclamation-triangle text-danger";
                default:
                case AgentStatus.Pending: return "fas fa-ellipsis-h text-muted";
            }
        }

        internal class AgentRow
        {
            public string Id {get; set;}
            public string Name {get; set;}
            public string Version {get; set;}
            public string[] Roles {get; set;}
            public string Class {get; set;}
        }
    }
}
