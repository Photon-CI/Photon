using Photon.Library;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Photon.Agent.Internal
{
    internal class Configuration
    {
        public static string AssemblyPath {get;}
        public static string Directory {get;}
        public static string Version {get;}

        public static int Parallelism => ConfigurationReader.AppSetting("parallelism", 1);
        private static string AgentFilePath => ConfigurationReader.AppSetting("agentFile", "agent.json");
        private static string WorkPath => ConfigurationReader.AppSetting("work", "Work");
        private static string ApplicationsPath => ConfigurationReader.AppSetting("applications", "Applications");
        private static string VariablesPath => ConfigurationReader.AppSetting("variables", "Variables");
        private static string RepositoryPath => ConfigurationReader.AppSetting("repository", "Repository");

        public static string AgentFile => FullPath(Directory, AgentFilePath);
        public static string WorkDirectory => FullPath(Directory, WorkPath);
        public static string ApplicationsDirectory => FullPath(Directory, ApplicationsPath);
        public static string VariablesDirectory => FullPath(Directory, VariablesPath);
        public static string RepositoryDirectory => FullPath(Directory, RepositoryPath);


        static Configuration()
        {
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyPath = Path.GetDirectoryName(assembly.Location);

            var _dir = ConfigurationReader.AppSetting("directory", AssemblyPath);
            Directory = Path.GetFullPath(GetRootDirectory(_dir));

            Version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
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
