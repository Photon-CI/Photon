using Photon.Library;
using System;
using System.IO;
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

            var _appData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var defaultDirectory = Path.Combine(_appData, "Photon", "Agent");

            var _dir = ConfigurationReader.AppSetting("directory", defaultDirectory);
            Directory = _dir == "." ? AssemblyPath : Path.GetFullPath(_dir);
        }

        private static string FullPath(params string[] paths)
        {
            var path = Path.Combine(paths);
            return Path.GetFullPath(path);
        }
    }
}
