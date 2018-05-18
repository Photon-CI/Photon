using Photon.Framework;
using Photon.Framework.Variables;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Library.Variables
{
    public class VariableSetDocumentManager
    {
        private readonly ConcurrentDictionary<string, VariableSetDocument> documents;
        private string path;

        public IEnumerable<string> AllKeys => documents.Keys;


        public VariableSetDocumentManager()
        {
            documents = new ConcurrentDictionary<string, VariableSetDocument>(StringComparer.OrdinalIgnoreCase);
        }

        public void Load(string path)
        {
            this.path = path;

            documents.Clear();

            if (!Directory.Exists(path)) return;

            foreach (var file in Directory.EnumerateFiles(path, "*.json", SearchOption.AllDirectories)) {
                var id = Path.GetFileNameWithoutExtension(file) ?? string.Empty;

                var document = new VariableSetDocument(file) {
                    Id = id,
                };

                documents[id] = document;
            }
        }

        public async Task Update(VariableSetDocument document, string prevId = null)
        {
            if (string.IsNullOrEmpty(document.Id))
                throw new ApplicationException("VariableSet ID cannot be null or empty!");

            var _prevId = prevId ?? document.Id;

            if (!string.Equals(document.Id, _prevId, StringComparison.OrdinalIgnoreCase)) {
                if (documents.TryGetValue(document.Id, out var _))
                    throw new ApplicationException("A VariableSet with the specified ID already exists!");

                if (!string.IsNullOrEmpty(_prevId) && documents.TryRemove(_prevId, out var _prevDoc)) {
                    File.Delete(_prevDoc.Filename);
                }
            }

            documents[document.Id] = document;

            if (string.IsNullOrEmpty(document.Filename)) {
                document.Filename = Path.Combine(path, $"{document.Id}.json");
            }

            await document.SaveJson();
        }

        public bool Remove(string id)
        {
            if (documents.TryRemove(id, out var document)) {
                File.Delete(document.Filename);
                return true;
            }

            return false;
        }

        public bool TryGet(string id, out VariableSetDocument document)
        {
            return documents.TryGetValue(id, out document);
        }

        public async Task<VariableSetCollection> GetCollection()
        {
            var collection = new VariableSetCollection(JsonSettings.Serializer);

            foreach (var document in documents.Values) {
                var data = await document.GetJson();
                collection.Json[document.Id] = data;
            }

            return collection;
        }
    }
}
