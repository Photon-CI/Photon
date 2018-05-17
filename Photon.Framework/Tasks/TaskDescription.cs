using System;

namespace Photon.Framework.Tasks
{
    [Serializable]
    public class TaskDescription
    {
        public string Name {get; set;}
        public string[] Roles {get; set;}
    }
}
