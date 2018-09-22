using Photon.Framework.Applications;

namespace Photon.Framework.Agent
{
    public interface IAgentBuildContext : IAgentContext
    {
        string PreBuildCommand {get;}
        string TaskName {get;}
        string GitRefspec {get;}
        uint BuildNumber {get;}
        string CommitHash {get;}
        string CommitAuthor {get;}
        string CommitMessage {get;}
        IApplicationReader Applications {get; set;}
    }
}
