using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace Photon.Agent.Internal.Applications
{
    [Serializable]
    public class Application
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Application));

        public string Name {get; set;}
        public string ProjectId {get; set;}
        public string Location {get; set;}
        public int? MaxRevisionCount {get; set;}
        public List<ApplicationRevision> Revisions {get; set;}


        public Application()
        {
            Revisions = new List<ApplicationRevision>();
        }

        public ApplicationRevision GetRevision(uint deploymentNumber)
        {
            return Revisions.FirstOrDefault(x => x.Matches(deploymentNumber));
        }

        public void ApplyRetentionPolicy(int maxCount)
        {
            var keepCount = MaxRevisionCount ?? maxCount;

            var revGroupList = Revisions
                .GroupBy(x => x.EnvironmentName ?? string.Empty)
                .ToArray();

            var errorList = new List<Exception>();

            foreach (var revGroup in revGroupList) {
                var revList = revGroup
                    .OrderByDescending(x => x.Time)
                    .Skip(keepCount).ToArray();

                foreach (var rev in revList) {
                    try {
                        Revisions.Remove(rev);
                        rev.Destroy();

                        Log.Info($"Pruned revision '{rev.DeploymentNumber}' of application '{Name}'.");
                    }
                    catch (Exception error) {
                        Log.Error($"Failed to prune revision '{rev.DeploymentNumber}' of application '{Name}'!", error);

                        errorList.Add(new ApplicationException($"Failed to remove revision '{rev.DeploymentNumber}'!", error));
                    }
                }
            }

            if (errorList.Any()) throw new AggregateException($"Errors occurred while removing revisions of application '{Name}'!", errorList);
        }
    }
}
