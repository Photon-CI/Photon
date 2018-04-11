namespace Photon.Framework.Server
{
    public interface IServerBuildContext : IServerContext
    {
        string GitRefspec {get; set;}
        string TaskName {get;}
        int BuildNumber {get; set;}
    }
}
