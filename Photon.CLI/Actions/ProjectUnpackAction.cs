using System;
using System.Threading.Tasks;

namespace Photon.CLI.Actions
{
    public class ProjectUnpackAction
    {
        public string PackageId {get; set;}
        public string PackageVersion {get; set;}
        public string OutputPath {get; set;}


        public Task Run()
        {
            throw new NotImplementedException();

            return Task.CompletedTask;
        }
    }
}
