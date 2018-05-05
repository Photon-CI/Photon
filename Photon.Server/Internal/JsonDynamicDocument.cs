using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Photon.Framework.Extensions;
using System;
using System.IO;

namespace Photon.Server.Internal
{
    internal class JsonDynamicDocument
    {
        public JsonSerializer Serializer {get; set;}
        public string Filename {get; set;}


        public JsonDynamicDocument()
        {
            Serializer = new JsonSerializer {
                Formatting = Formatting.Indented,
            };
        }

        public void Load(Action<JObject> loadAction)
        {
            if (!File.Exists(Filename)) return;

            using (var stream = File.Open(Filename, FileMode.Open, FileAccess.Read)) {
                var document = Serializer.Deserialize(stream);

                loadAction?.Invoke(document);
            }
        }

        public void Update(Action<JObject> updateAction)
        {
            var path = Path.GetDirectoryName(Configuration.ServerFile);

            if (path != null && !Directory.Exists(path))
                Directory.CreateDirectory(path);

            using (var stream = File.Open(Configuration.ServerFile, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
                var serializer = new JsonSerializer();
                JObject document = serializer.Deserialize(stream, true) ?? new JObject();

                updateAction(document);

                stream.SetLength(0);
                serializer.Serialize(stream, document);
            }
        }

        public void Remove(Func<JObject, bool> removeAction)
        {
            if (!File.Exists(Filename)) return;

            using (var stream = File.Open(Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
                var serializer = new JsonSerializer();
                JObject document = serializer.Deserialize(stream, true) ?? new JObject();

                if (!removeAction(document)) return;

                stream.SetLength(0);
                serializer.Serialize(stream, document);
            }
        }
    }
}
