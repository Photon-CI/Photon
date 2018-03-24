using Photon.Library;
using System.IO;
using System.Reflection;

namespace Photon.Server.Internal
{
    internal class Configuration
    {
        public static string AssemblyPath {get;}

        public static string DefinitionFilename => ConfigurationReader.AppSetting("definition");
        public static string ProjectDirectory => ConfigurationReader.AppSetting("projects");


        static Configuration()
        {
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyPath = Path.GetDirectoryName(assembly.Location);
        }
    }
}
