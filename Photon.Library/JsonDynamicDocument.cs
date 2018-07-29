using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Photon.Framework.Extensions;
using Photon.Framework.Tools;
using System;
using System.IO;

namespace Photon.Library
{
    public class JsonDynamicDocument
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

        public void Load(Action<JToken> loadAction)
        {
            if (!File.Exists(Filename)) return;

            using (var stream = File.Open(Filename, FileMode.Open, FileAccess.Read)) {
                var document = Serializer.Deserialize(stream);

                loadAction?.Invoke(document);
            }
        }

        public void Update(Action<JToken> updateAction)
        {
            PathEx.CreateFilePath(Filename);

            AlterDocument(document => {
                updateAction(document);
                return true;
            });
        }

        public void Remove(Func<JToken, bool> removeAction)
        {
            if (!File.Exists(Filename)) return;

            AlterDocument(removeAction);
        }

        private void AlterDocument(Func<JToken, bool> alterFunc)
        {
            PathEx.CreateFilePath(Filename);

            using (var stream = File.Open(Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
                var document = Serializer.Deserialize(stream, true);

                if (!alterFunc(document)) return;

                stream.SetLength(0);
                JsonSerializerExtensions.Serialize(Serializer, stream, document);
            }
        }
    }
}
