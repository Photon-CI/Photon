namespace Photon.Agent.Internal.Git
{
    internal class RepositorySource
    {
        private readonly object lockHandle;
        private bool isLocked;

        public string RepositoryUrl {get;}
        public string RepositoryPath {get;}


        public RepositorySource(string url, string path)
        {
            this.RepositoryUrl = url;
            this.RepositoryPath = path;

            lockHandle = new object();
        }

        public void Initialize()
        {
            //
        }

        public bool TryBegin(out RepositoryHandle handle)
        {
            lock (lockHandle) {
                if (isLocked) {
                    handle = null;
                    return false;
                }

                isLocked = true;
                handle = new RepositoryHandle(this, End);
                return true;
            }
        }

        private void End()
        {
            lock (lockHandle) {
                isLocked = false;
            }
        }
    }
}
