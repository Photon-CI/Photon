using Photon.Library;
using System;
using System.IO;
using System.Reflection;

namespace Photon.Server.Internal
{
    internal class Configuration
    {
        private static readonly string DefaultDirectory;

        public static string AssemblyPath {get;}

        public static string Directory => ConfigurationReader.AppSetting("directory", DefaultDirectory);
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
            DefaultDirectory = Path.Combine(_appData, "Photon", "Server");
        }
    }
}
