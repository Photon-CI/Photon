using Photon.Library;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Photon.Agent.Internal
{
    internal class Configuration
    {
        public static string AssemblyPath {get;}
        public static string Directory {get;}

        public static int Parallelism => ConfigurationReader.AppSetting("parallelism", 1);
        private static string AgentFilePath => ConfigurationReader.AppSetting("agentFile", "agent.json");
        private static string WorkPath => ConfigurationReader.AppSetting("work", "Work");
        private static string ApplicationsPath => ConfigurationReader.AppSetting("applications", "Applications");

        public static string AgentFile => FullPath(Directory, AgentFilePath);
        public static string WorkDirectory => FullPath(Directory, WorkPath);
        public static string ApplicationsDirectory => FullPath(Directory, ApplicationsPath);


        static Configuration()
        {
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyPath = Path.GetDirectoryName(assembly.Location);

            var _dir = ConfigurationReader.AppSetting("directory", AssemblyPath);
            Directory = Path.GetFullPath(GetRootDirectory(_dir));
        }

        private static string FullPath(params string[] paths)
        {
            var path = Path.Combine(paths);
            return Path.GetFullPath(path);
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
