using System;
using System.IO;

namespace Photon.CLI.Internal
{
    internal class CommandContext
    {
        public ServerCollection Servers {get; private set;}


        public CommandContext()
        {
            Servers = new ServerCollection();
        }

        public void Initialize()
        {
            var commonDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var appDataPath = Path.Combine(commonDataPath, "Photon", "CLI");

            var serverDefinitionsFilename = Path.Combine(appDataPath, "servers.json");
            Servers = File.Exists(serverDefinitionsFilename)
                ? ServerCollection.Load(serverDefinitionsFilename)
                : new ServerCollection(serverDefinitionsFilename);
        }
    }
}
