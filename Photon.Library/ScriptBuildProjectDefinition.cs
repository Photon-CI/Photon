namespace Photon.Library
{
    public class ScriptBuildProjectDefinition
    {
        public string Name { get; set; }

        public ProjectSourceDefinition Source { get; set; }

        //public ProjectEnvironmentDefinition Environment { get; set; }

        public ProjectJobDefinition Job { get; set; }


        public ScriptBuildProjectDefinition()
        {
            Source = new ProjectSourceDefinition();
            //Environment = new ProjectEnvironmentDefinition();
            Job = new ProjectJobDefinition();
        }
    }
}
