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
        private static string ProjectPackagePath => ConfigurationReader.AppSetting("projectPackages", "ProjectPackages");

        public static string ServerFile => FullPath(Directory, ServerFilePath);
        public static string ProjectsFile => FullPath(Directory, ProjectsFilePath);
        public static string WorkDirectory => FullPath(Directory, WorkPath);
        public static string ProjectDataDirectory => FullPath(Directory, ProjectDataPath);
        public static string ProjectPackageDirectory => FullPath(Directory, ProjectPackagePath);


        static Configuration()
        {
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyPath = Path.GetDirectoryName(assembly.Location);

            var _appData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var defaultDirectory = Path.Combine(_appData, "Photon", "Server");

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
