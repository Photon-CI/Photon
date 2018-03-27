namespace Photon.Library
{
    public class ScriptBuildDefinition
    {
        public string SessionId { get; set; }

        public ScriptBuildProjectDefinition Project {get; set;}


        public ScriptBuildDefinition()
        {
            Project = new ScriptBuildProjectDefinition();
        }
    }
}
