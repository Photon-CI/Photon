using Newtonsoft.Json;
using Photon.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Photon.CLI.Internal
{
    public class ServerCollection
    {
        private string filename;

        [JsonProperty("definitions")]
        public List<ServerDefinition> Definitions {get;}


        public ServerCollection()
        {
            Definitions = new List<ServerDefinition>();
        }

        public ServerCollection(string filename) : this()
        {
            this.filename = filename;
        }

        public static ServerCollection Load(string filename)
        {
            ServerCollection collection;
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read)) {
                var serializer = new JsonSerializer();
                collection = serializer.Deserialize<ServerCollection>(stream);
            }

            collection.filename = filename;
            return collection;
        }

        public void Save()
        {
            var filePath = Path.GetDirectoryName(filename);

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            using (var stream = File.Open(filename, FileMode.Create, FileAccess.Write)) {
                var serializer = new JsonSerializer();
                serializer.Serialize(stream, this);
            }
        }

        public void Add(ServerDefinition definition)
        {
            Definitions.Add(definition);
        }

        public ServerDefinition Get(string name)
        {
            return Definitions.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public bool RemoveByName(string name)
        {
            return Definitions.RemoveAll(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;
        }

        public ServerDefinition GetPrimary()
        {
            return Definitions.FirstOrDefault(x => x.Primary);
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
