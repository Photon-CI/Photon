using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Framework.Applications
{
    [Serializable]
    public class Application
    {
        public string Name {get; set;}
        public string ProjectId {get; set;}
        public string Location {get; set;}
        public List<ApplicationRevision> Revisions {get; set;}


        public Application()
        {
            Revisions = new List<ApplicationRevision>();
        }

        public bool Matches(string projectId, string appName)
        {
            return string.Equals(projectId, ProjectId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(appName, Name, StringComparison.OrdinalIgnoreCase);
        }

        public ApplicationRevision GetRevision(uint deploymentNumber)
        {
            return Revisions.FirstOrDefault(x => x.Matches(deploymentNumber));
        }

        public void ApplyRetentionPolicy(int maxCount)
        {
            var revList = Revisions
                .OrderByDescending(x => x.Time)
                .Skip(maxCount).ToArray();

            var errorList = new List<Exception>();

            foreach (var rev in revList) {
                try {
                    rev.Destroy();
                }
                catch (Exception error) {
                    errorList.Add(error);
                }
            }

            if (errorList.Any()) throw new AggregateException($"Errors occurred while removing revisions of application '{Name}'!", errorList);
        }
    }
}
