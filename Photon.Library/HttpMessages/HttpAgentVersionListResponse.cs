namespace Photon.Library.HttpMessages
{
    public class HttpAgentVersionListResponse
    {
        public string SessionId {get; set;}
        public AgentVersionResponse[] VersionList {get; set;}
    }

    public class AgentVersionResponse
    {
        public string AgentId {get; set;}
        public string AgentVersion {get; set;}
        public string Exception {get; set;}
    }
}
