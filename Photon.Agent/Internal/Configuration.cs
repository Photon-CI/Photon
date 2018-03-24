using Photon.Library;
using System.IO;
using System.Reflection;

namespace Photon.Agent.Internal
{
    internal class Configuration
    {
        public static string AssemblyPath {get;}

        public static string HttpHost => ConfigurationReader.AppSetting("http.host");
        public static int HttpPort => ConfigurationReader.AppSetting<int>("http.port");
        public static string HttpPath => ConfigurationReader.AppSetting("http.path");


        static Configuration()
        {
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyPath = Path.GetDirectoryName(assembly.Location);
        }
    }
}
