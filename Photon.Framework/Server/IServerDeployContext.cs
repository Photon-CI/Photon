﻿namespace Photon.Framework.Server
{
    public interface IServerDeployContext : IServerContext
    {
        uint DeploymentNumber {get;}
        string ProjectPackageId {get;}
        string ProjectPackageVersion {get;}
        string EnvironmentName {get;}
        string ScriptName {get;}
    }
}
