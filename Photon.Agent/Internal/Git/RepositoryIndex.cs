using log4net;
using Newtonsoft.Json;
using Photon.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Photon.Agent.Internal.Git
{
    internal class RepositoryIndex
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RepositoryIndex));

        private readonly object lockHandle;
        private readonly Dictionary<string, RepositoryIndexEntry> entries;
        private string filename;


        public RepositoryIndex()
        {
            lockHandle = new object();
            entries = new Dictionary<string, RepositoryIndexEntry>(StringComparer.OrdinalIgnoreCase);
        }

        public void Initialize(string filename)
        {
            this.filename = filename;

            if (!File.Exists(filename)) return;

            RepositoryIndexEntry[] data;
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader)) {
                data = JsonSettings.Serializer.Deserialize<RepositoryIndexEntry[]>(jsonReader);
            }

            foreach (var item in data)
                entries[item.Url] = item;
        }

        public RepositoryIndexEntry GetOrCreate(string url)
        {
            lock (lockHandle) {
                if (entries.TryGetValue(url, out var entry))
                    return entry;

                entry = new RepositoryIndexEntry(url);
                entries[url] = entry;

                try {
                    Save();
                }
                catch (Exception error) {
                    Log.Error("Failed to save repository index!", error);
                }

                return entry;
            }
        }

        private void Save()
        {
            var path = Path.GetDirectoryName(filename);

            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);

            using (var stream = File.Open(filename, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(writer)) {
                JsonSettings.Serializer.Serialize(jsonWriter, entries.Values.ToArray());
            }
        }
    }
}
