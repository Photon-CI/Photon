using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Framework.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Photon.CLI.Internal
{
    public class ServerCollection
    {
        private string filename;

        public List<PhotonServerDefinition> Definitions {get;}


        public ServerCollection()
        {
            Definitions = new List<PhotonServerDefinition>();
        }

        public ServerCollection(string filename) : this()
        {
            this.filename = filename;
        }

        public static ServerCollection Load(string filename)
        {
            ServerCollection collection;
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read)) {
                collection = JsonSettings.Serializer.Deserialize<ServerCollection>(stream);
            }

            collection.filename = filename;
            return collection;
        }

        public void Save()
        {
            PathEx.CreateFilePath(filename);

            using (var stream = File.Open(filename, FileMode.Create, FileAccess.Write)) {
                JsonSettings.Serializer.Serialize(stream, this);
            }
        }

        public void Add(PhotonServerDefinition definition)
        {
            Definitions.Add(definition);
        }

        public bool TryGet(string name, out PhotonServerDefinition server)
        {
            server = Definitions.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            return server != null;
        }

        public PhotonServerDefinition Get(string name)
        {
            PhotonServerDefinition server;
            if (!string.IsNullOrEmpty(name)) {
                server = Definitions.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

                return server ?? new PhotonServerDefinition {
                    Name = "Custom",
                    Url = name,
                };

                //if (server == null) throw new ApplicationException($"Server '{name}' could not be found!");
            }

            server = Definitions.FirstOrDefault(x => x.Primary);

            return server ?? throw new ApplicationException("No primary Server could not be found!");
        }

        public bool RemoveByName(string name)
        {
            return Definitions.RemoveAll(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;
        }

        public void SetPrimary(string name)
        {
            var newPrimaryDef = Get(name);
            if (newPrimaryDef == null) throw new ApplicationException($"Server '{name}' not found!");

            foreach (var definition in Definitions)
                definition.Primary = definition == newPrimaryDef;
        }
    }
}
