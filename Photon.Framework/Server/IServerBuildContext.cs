namespace Photon.Framework.Server
{
    public interface IServerBuildContext : IServerContext
    {
        string PreBuild {get; set;}
        string GitRefspec {get; set;}
        string TaskName {get;}
        int BuildNumber {get; set;}
    }
}
