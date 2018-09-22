using Photon.Communication;
using Photon.Framework.Agent;
using Photon.Framework.Tools.Content;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Worker.Internal.Session
{
    internal abstract class WorkerSession : IDisposable
    {
        private readonly TaskCompletionSource<object> completeTask;

        public IAgentContext Context {get; set;}
        public MessageTransceiver Transceiver {get; set;}
        public SessionOutput Output {get; private set;}


        protected WorkerSession()
        {
            completeTask = new TaskCompletionSource<object>();
        }

        public virtual void Dispose()
        {
            Output?.Dispose();
            //Transceiver?.Dispose();
        }

        public virtual Task Initialize(CancellationToken token = default)
        {
            Output = new SessionOutput(Transceiver, Context.ServerSessionId, Context.AgentSessionId, Context.ClientId);

            return Task.CompletedTask;
        }

        public virtual Task Run(CancellationToken token = default)
        {
            //...

            return completeTask.Task;
        }

        public virtual Task Release(CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        /// <summary></summary>
        /// <returns>The full filename of the assembly copy.</returns>
        protected string ShadowCopyAssembly()
        {
            var fullAssemblyFilename = Path.Combine(Context.ContentDirectory, Context.AssemblyFilename);

            if (!File.Exists(fullAssemblyFilename)) {
                Output.WriteLine($"The assembly file '{fullAssemblyFilename}' could not be found!", ConsoleColor.DarkYellow);
                throw new FileNotFoundException($"The assembly file '{fullAssemblyFilename}' could not be found!", fullAssemblyFilename);
            }

            try {
                var sourcePath = Path.GetDirectoryName(Context.AssemblyFilename);
                var assemblyName = Path.GetFileName(fullAssemblyFilename);
                var assemblyCopyFilename = Path.Combine(Context.BinDirectory, assemblyName);
                CopyDirectory(sourcePath, Context.BinDirectory);

                return assemblyCopyFilename;
            }
            catch (Exception error) {
                Output.WriteLine($"Failed to shadow-copy assembly '{fullAssemblyFilename}'!", ConsoleColor.DarkYellow);
                throw new ApplicationException($"Failed to shadow-copy assembly '{fullAssemblyFilename}'!", error);
            }
        }

        private static void CopyDirectory(string sourcePath, string destPath)
        {
            var filter = new ContentFilter {
                SourceDirectory = sourcePath,
                DestinationDirectory = destPath,
                DirectoryAction = (src, dest) => Directory.CreateDirectory(dest),
                FileAction = File.Copy,
                IgnoredDirectories = {
                    ".git",
                },
            };

            filter.Run();
        }
    }
}
