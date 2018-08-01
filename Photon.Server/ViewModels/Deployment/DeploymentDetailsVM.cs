using Photon.Framework.Packages;
using Photon.Server.Internal;
using Photon.Server.Internal.Deployments;
using System;
using System.Collections.Generic;

namespace Photon.Server.ViewModels.Deployment
{
    internal class DeploymentDetailsVM : ServerViewModel
    {
        public string ProjectId {get; set;}
        public uint DeploymentNumber {get; set;}
        public string SessionId {get; private set;}
        public string ProjectName {get; private set;}
        public string DeploymentCreated {get; private set;}
        public string DeploymentDuration {get; private set;}
        public string DeploymentException {get; private set;}
        public string PackageId {get; private set;}
        public string PackageVersion {get; private set;}
        public string Environment {get; private set;}
        public PackageReference[] ApplicationPackages {get; private set;}
        public List<object> Artifacts {get; private set;}
        public string IconClass {get; private set;}
        public bool IsRunning {get; private set;}


        public void Build()
        {
            IconClass = "fas fa-ellipses-h text-muted";

            if (!string.IsNullOrEmpty(ProjectId) && PhotonServer.Instance.Projects.TryGet(ProjectId, out var project)) {
                ProjectName = project.Description.Name;

                if (project.Deployments.TryGet(DeploymentNumber, out var deploymentData)) {
                    DeploymentCreated = deploymentData.Created.ToLocalTime().ToString("F");
                    DeploymentDuration = deploymentData.Duration?.ToString("g");
                    DeploymentException = deploymentData.Exception;

                    SessionId = deploymentData.ServerSessionId;
                    PackageId = deploymentData.PackageId;
                    PackageVersion = deploymentData.PackageVersion;
                    Environment = deploymentData.EnvironmentName;
                    ApplicationPackages = deploymentData.ApplicationPackages;

                    Artifacts = new List<object>();
                    // TODO

                    IconClass = GetIconClass(deploymentData);

                    if (!string.IsNullOrEmpty(deploymentData.ServerSessionId) && PhotonServer.Instance.Sessions.TryGet(deploymentData.ServerSessionId, out var session)) {
                        IsRunning = !session.IsComplete;
                    }
                }
                else {
                    Errors.Add(new ApplicationException($"Deployment '{DeploymentNumber}' not found!"));
                }
            }
            else {
                Errors.Add(new ApplicationException($"Project '{ProjectId}' not found!"));
            }
        }

        private string GetIconClass(DeploymentData data)
        {
            if (data.IsCancelled) return "fas fa-trash text-warning";
            if (!data.IsComplete) return "fas fa-spinner fa-spin text-info";
            if (data.IsSuccess) return "fas fa-check text-success";
            return "fas fa-exclamation-triangle text-danger";
        }
    }
}
