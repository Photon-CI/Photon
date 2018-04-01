using Photon.Library;
using System.IO;
using System.Reflection;

namespace Photon.Server.Internal
{
    internal class Configuration
    {
        private static readonly string DefaultProjectDirectory;
        private static readonly string DefaultWorkDirectory;

        public static string AssemblyPath {get;}

        public static string DefinitionFilename => ConfigurationReader.AppSetting("definition");
        public static string ProjectDirectory => ConfigurationReader.AppSetting("directory.projects");
        public static string WorkDirectory => ConfigurationReader.AppSetting("directory.work");


        static Configuration()
        {
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyPath = Path.GetDirectoryName(assembly.Location);

            DefaultProjectDirectory = Path.Combine(AssemblyPath, "Projects");
            DefaultWorkDirectory = Path.Combine(AssemblyPath, "Work");
        }
    }
}
