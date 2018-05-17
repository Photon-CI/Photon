using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
        }

        public void Load(Action<dynamic> loadAction)
        {
            if (!File.Exists(Filename)) return;

            using (var stream = File.Open(Filename, FileMode.Open, FileAccess.Read)) {
                dynamic document = Serializer.Deserialize(stream);

                loadAction?.Invoke(document);
            }
        }

        public void Update(Action<dynamic> updateAction)
        {
            var path = Path.GetDirectoryName(Filename);

            if (path != null && !Directory.Exists(path))
                Directory.CreateDirectory(path);

            using (var stream = File.Open(Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
                dynamic document = Serializer.Deserialize(stream, true);

                updateAction(document);

                stream.SetLength(0);
                JsonSerializerExtensions.Serialize(Serializer, stream, document);
            }
        }

        public void Remove(Func<dynamic, bool> removeAction)
        {
            if (!File.Exists(Filename)) return;

            using (var stream = File.Open(Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
                JObject document = Serializer.Deserialize(stream, true);

                if (!removeAction(document)) return;

                stream.SetLength(0);
                Serializer.Serialize(stream, document);
            }
        }
    }
}
