using Photon.Server.Internal.Scripts;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal class ServerDeploySession : ServerSessionBase
    {
        public ServerDeployContext Context {get;}


        public ServerDeploySession(ServerDeployContext context)
        {
            this.Context = context;
        }

        public override void PrepareWorkDirectory()
        {
            base.PrepareWorkDirectory();

            // TODO: Copy release package
            //CopyPackage();
        }

        public override async Task RunAsync()
        {
            var assemblyFilename = Path.Combine(Context.WorkDirectory, Context.AssemblyFile);
            if (!File.Exists(assemblyFilename))
                throw new FileNotFoundException($"The assembly file '{assemblyFilename}' could not be found!");

            Domain.Initialize(assemblyFilename);

            var result = await Domain.RunDeployScript(Context);
            if (!result.Successful) throw new ApplicationException(result.Message);
        }

        private void CopyPackage(string packageName, string version, string outputDirectory)
        {
            //...
        }
    }
}
