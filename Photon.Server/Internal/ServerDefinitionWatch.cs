using System;
using log4net;
using Photon.Framework;
using Photon.Framework.Extensions;
using System.IO;

namespace Photon.Server.Internal
{
    internal class ServerDefinitionWatch : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServerDefinitionWatch));

        private readonly string filename;
        private readonly ServerDefinition defaultDefinition;
        private FileSystemWatcher fsWatcher;

        public ServerDefinition Definition {get; private set;}


        public ServerDefinitionWatch()
        {
            var file = Configuration.ServerFile ?? "server.json";
            file = Path.Combine(Configuration.AssemblyPath, file);
            filename = Path.GetFullPath(file);

            defaultDefinition = new ServerDefinition {
                Http = {
                    Host = "*",
                    Port = 8082,
                    Path = "/photon/server",
                },
            };
        }

        public void Dispose()
        {
            fsWatcher?.Dispose();
        }

        public void Initialize()
        {
            LoadFile();

            var path = Path.GetDirectoryName(filename);

            fsWatcher = new FileSystemWatcher {
                Path = path,
                Filter = "server.json",
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true,
            };

            fsWatcher.Changed += FsWatcher_OnChanged;
        }

        private void LoadFile()
        {
            if (!File.Exists(filename)) {
                Definition = defaultDefinition;
                Log.Warn($"Server Definition not found! {filename}");
                return;
            }

            Log.Debug($"Loading Server Definition: {filename}");

            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                Definition = JsonSettings.Serializer.Deserialize<ServerDefinition>(stream);
            }
        }

        private void FsWatcher_OnChanged(object sender, FileSystemEventArgs e)
        {
            Log.Debug("Server Definition Modified.");

            // TODO
        }
    }
}
