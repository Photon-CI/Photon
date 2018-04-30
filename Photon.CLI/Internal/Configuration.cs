using Photon.Library;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Photon.CLI.Internal
{
    internal class Configuration
    {
        public static string DownloadUrl => ConfigurationReader.AppSetting("url.downloads", "http://download.photon.ci");

        public static string AssemblyPath {get;}
        public static string Directory {get;}
        public static string Version {get;}


        static Configuration()
        {
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyPath = Path.GetDirectoryName(assembly.Location);

            var _dir = ConfigurationReader.AppSetting("directory", AssemblyPath);
            Directory = Path.GetFullPath(GetRootDirectory(_dir));

            Version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
        }

        private static string GetRootDirectory(string path)
        {
            var pathParts = path.Split(Path.DirectorySeparatorChar).ToList();

            if (pathParts.Count >= 1 && pathParts[0] == ".") {
                pathParts[0] = AssemblyPath;
                return Path.Combine(pathParts.ToArray());
            }

            return path;
        }
    }
}
