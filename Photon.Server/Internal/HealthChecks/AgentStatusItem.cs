namespace Photon.Server.Internal.HealthChecks
{
    internal class AgentStatusItem
    {
        public string AgentId {get; set;}
        public string AgentName {get; set;}
        public AgentStatus Status {get; set;}
        public string[] Warnings {get; set;}
        public string[] Errors {get; set;}
    }
}
