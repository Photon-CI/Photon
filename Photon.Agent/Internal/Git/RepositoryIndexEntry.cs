using System;

namespace Photon.Agent.Internal.Git
{
    internal class RepositoryIndexEntry
    {
        public string Id {get; set;}
        public string Url {get; set;}


        public RepositoryIndexEntry() {}

        public RepositoryIndexEntry(string url)
        {
            this.Url = url;

            Id = Guid.NewGuid().ToString("N");
        }
    }
}
