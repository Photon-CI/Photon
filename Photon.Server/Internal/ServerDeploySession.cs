using Photon.Framework.Projects;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Server.Internal
{
    internal class ServerDeploySession : ServerSessionBase
    {
        public ServerDeploySession(ProjectDefinition project, ProjectJobDefinition job, string releaseVersion) : base(project, job)
        {
            Context.ReleaseVersion = releaseVersion;
        }

        public override void PrepareWorkDirectory()
        {
            base.PrepareWorkDirectory();

            // TODO: Copy release package
            //CopyPackage();
        }

        public override async Task RunAsync()
        {
            var assemblyFilename = Path.Combine(Context.WorkDirectory, Context.Job.Assembly);
            if (!File.Exists(assemblyFilename))
                throw new FileNotFoundException($"The assembly file '{assemblyFilename}' could not be found!");

            Domain.Initialize(assemblyFilename);

            var result = await Domain.RunScript(Context);
            if (!result.Successful) throw new ApplicationException(result.Message);
        }

        private void CopyPackage(string packageName, string version, string outputDirectory)
        {
            //...
        }
    }
}
