namespace Photon.Framework.Server
{
    public interface IServerDeployContext : IServerContext
    {
        string ProjectPackageId {get;}
        string ProjectPackageVersion {get;}
        string ScriptName {get;}
    }
}
