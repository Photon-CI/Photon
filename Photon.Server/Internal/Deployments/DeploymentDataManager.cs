using log4net;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Framework.Tools;
using System;
using System.Collections.Generic;
using System.IO;

namespace Photon.Server.Internal.Deployments
{
    internal class DeploymentDataManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DeploymentDataManager));

        private readonly string deploymentPath;
        private readonly Dictionary<uint, DeploymentData> data;

        public IEnumerable<uint> AllNumbers => data.Keys;
        public IEnumerable<DeploymentData> AllDeployments => data.Values;


        public DeploymentDataManager(string deploymentPath)
        {
            this.deploymentPath = deploymentPath;

            data = new Dictionary<uint, DeploymentData>();
        }

        public void Load()
        {
            if (!Directory.Exists(deploymentPath)) return;

            foreach (var path in Directory.EnumerateDirectories(deploymentPath, "*", SearchOption.TopDirectoryOnly)) {
                var path_name = Path.GetFileName(path);

                if (!uint.TryParse(path_name, out var deploymentNumber)) {
                    Log.Warn($"Failed to parse deployment directory '{path_name}'!");
                    continue;
                }

                var indexFilename = Path.Combine(path, "index.json");
                DeploymentData deployData;

                if (File.Exists(indexFilename)) {
                    using (var stream = File.Open(indexFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                        deployData = JsonSettings.Serializer.Deserialize<DeploymentData>(stream);

                        //buildData.Number = buildNumber;
                        deployData.ContentPath = path;
                    }
                }
                else {
                    deployData = new DeploymentData {
                        Number = deploymentNumber,
                        ContentPath = path,
                    };
                }

                data[deploymentNumber] = deployData;
            }
        }

        public DeploymentData New(uint number)
        {
            var _path = Path.Combine(deploymentPath, number.ToString());
            PathEx.CreatePath(_path);

            var deployData = new DeploymentData {
                Number = number,
                ContentPath = _path,
                Created = DateTime.UtcNow,
            };

            data[number] = deployData;
            return deployData;
        }

        public bool TryGet(uint number, out DeploymentData deployData)
        {
            return data.TryGetValue(number, out deployData);
        }
    }
}
