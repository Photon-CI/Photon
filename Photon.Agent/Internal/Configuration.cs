using Photon.Library;
using System.IO;
using System.Reflection;

namespace Photon.Agent.Internal
{
    internal class Configuration
    {
        public static string AssemblyPath {get;}

        public static string DefinitionPath => ConfigurationReader.AppSetting("definition.path");


        static Configuration()
        {
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyPath = Path.GetDirectoryName(assembly.Location);
        }
    }
}
