namespace Photon.Server.Internal.GitHub
{
    public class GithubCommit
    {
        public string RepositoryUrl {get; set;}
        public string StatusesUrl {get; set;}
        public string Refspec {get; set;}
        public bool IsTag {get; set;}
        public string Sha {get; set;}
    }
}
