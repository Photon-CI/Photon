using System;
using System.Collections.Concurrent;
using System.IO;

namespace Photon.Agent.Internal.Git
{
    internal class RepositorySourceManager
    {
        private readonly RepositoryIndex index;
        private readonly ConcurrentDictionary<string, RepositorySource> sources;

        public string RepositorySourceDirectory {get; set;}


        public RepositorySourceManager()
        {
            index = new RepositoryIndex();
            sources = new ConcurrentDictionary<string, RepositorySource>(StringComparer.OrdinalIgnoreCase);
        }

        public void Initialize()
        {
            var indexFilename = Path.Combine(RepositorySourceDirectory, "index.json");

            index.Initialize(indexFilename);
        }

        public RepositorySource GetOrCreate(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var entry = index.GetOrCreate(url);

            return sources.GetOrAdd(entry.Id, _ => CreateSource(entry));
        }

        //public bool TryGet(string name, out RepositorySource repository)
        //{
        //    return sources.TryGetValue(name, out repository);
        //}

        private RepositorySource CreateSource(RepositoryIndexEntry entry)
        {
            var repositoryPath = Path.Combine(RepositorySourceDirectory, entry.Id);

            if (!Directory.Exists(repositoryPath))
                Directory.CreateDirectory(repositoryPath);

            return new RepositorySource(entry.Url, repositoryPath) {
                //...
            };
        }
    }
}
