using System;

namespace Photon.Framework.Projects
{
    [Serializable]
    public class Project
    {
        public string Id {get; set;}
        public string Name {get; set;}
        public string Description {get; set;}
        public string SourceType {get; set;}
        public string SourcePath {get; set;}
        public string SourceUrl {get; set;}
        public string SourceUsername {get; set;}
        public string SourcePassword {get; set;}
    }
}
