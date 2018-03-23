using Photon.Library;
using System.IO;
using System.Reflection;

namespace Photon.Server.Internal
{
    internal class Configuration
    {
        public static string AssemblyPath {get;}

        public static string HttpPath => ConfigurationReader.AppSetting("http.path");


        static Configuration()
        {
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyPath = Path.GetDirectoryName(assembly.Location);
        }
    }
}
