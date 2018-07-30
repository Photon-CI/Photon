namespace Photon.Server.Internal.HealthChecks
{
    internal enum AgentStatus
    {
        Pending,
        Ok,
        Warning,
        Error,
        Disconnected,
    }
}
