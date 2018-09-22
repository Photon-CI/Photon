using Photon.Library;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Photon.Worker.Internal
{
    internal class Configuration
    {
        public static string AssemblyPath {get;}
        public static string Version {get;}

        public static int Parallelism => ConfigurationReader.AppSetting("parallelism", 1);


        static Configuration()
        {
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyPath = Path.GetDirectoryName(assembly.Location);

            Version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
        }
    }
}
