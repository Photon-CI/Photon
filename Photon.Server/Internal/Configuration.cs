using Photon.Library;
using System;
using System.IO;
using System.Reflection;

namespace Photon.Server.Internal
{
    internal class Configuration
    {
        public static string AssemblyPath {get;}
        public static string Directory {get;}

        public static int Parallelism => ConfigurationReader.AppSetting("parallelism", 1);
        private static string ServerFilePath => ConfigurationReader.AppSetting("serverFile", "server.json");
        private static string ProjectsFilePath => ConfigurationReader.AppSetting("projectsFile", "projects.json");
        private static string WorkPath => ConfigurationReader.AppSetting("work", "Work");
        private static string ProjectDataPath => ConfigurationReader.AppSetting("projectData", "ProjectData");

        public static string ServerFile => Path.Combine(Directory, ServerFilePath);
        public static string ProjectsFile => Path.Combine(Directory, ProjectsFilePath);
        public static string WorkDirectory => Path.Combine(Directory, WorkPath);
        public static string ProjectDataDirectory => Path.Combine(Directory, ProjectDataPath);


        static Configuration()
        {
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyPath = Path.GetDirectoryName(assembly.Location);

            var _appData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var defaultDirectory = Path.Combine(_appData, "Photon", "Server");

            var _dir = ConfigurationReader.AppSetting("directory", defaultDirectory);;
            Directory = _dir == "." ? AssemblyPath : Path.GetFullPath(_dir);
        }
    }
}
