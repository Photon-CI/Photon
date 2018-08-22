namespace Photon.Framework.Agent
{
    public interface IAgentBuildContext : IAgentContext
    {
        string TaskName {get;}
        string GitRefspec {get;}
        uint BuildNumber {get;}
        string CommitHash {get;}
        string CommitAuthor {get;}
        string CommitMessage {get;}
    }
}
