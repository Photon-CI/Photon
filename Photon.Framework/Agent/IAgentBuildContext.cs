namespace Photon.Framework.Agent
{
    public interface IAgentBuildContext : IAgentContext
    {
        string TaskName {get;}
        string GitRefspec {get;}
        uint BuildNumber {get;}
    }
}
