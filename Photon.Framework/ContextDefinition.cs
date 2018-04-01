using Newtonsoft.Json;

namespace Photon.Framework
{
    public class ContextDefinition
    {
        [JsonProperty("project")]
        public ContextProjectDefinition Project {get; set;}

        [JsonProperty("task")]
        public ContextTaskDefinition Task {get; set;}


        public ContextDefinition()
        {
            Project = new ContextProjectDefinition();
            Task = new ContextTaskDefinition();
        }
    }
}
