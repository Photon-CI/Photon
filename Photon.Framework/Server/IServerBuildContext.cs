namespace Photon.Framework.Server
{
    public interface IServerBuildContext : IServerContext
    {
        uint BuildNumber {get;}
        string PreBuild {get;}
        string GitRefspec {get;}
        string TaskName {get;}
    }
}
