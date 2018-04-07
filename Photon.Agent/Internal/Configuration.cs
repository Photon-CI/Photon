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

        public static string AgentFile => Path.Combine(Directory, AgentFilePath);
        public static string WorkDirectory => Path.Combine(Directory, WorkPath);


        static Configuration()
        {
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyPath = Path.GetDirectoryName(assembly.Location);

            var _appData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var defaultDirectory = Path.Combine(_appData, "Photon", "Agent");

            var _dir = ConfigurationReader.AppSetting("directory", defaultDirectory);;
            Directory = _dir == "." ? AssemblyPath : Path.GetFullPath(_dir);
        }
    }
}
