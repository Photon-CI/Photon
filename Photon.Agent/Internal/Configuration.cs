using Photon.Library;
using System.IO;
using System.Reflection;

namespace Photon.Agent.Internal
{
    internal class Configuration
    {
        private static readonly string DefaultWorkDirectory;

        public static string AssemblyPath {get;}

        public static string DefinitionPath => ConfigurationReader.AppSetting("definition.path");
        public static string WorkDirectory => ConfigurationReader.AppSetting("directory.work");


        static Configuration()
        {
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyPath = Path.GetDirectoryName(assembly.Location);

            DefaultWorkDirectory = Path.Combine(AssemblyPath, "Work");
        }
    }
}
