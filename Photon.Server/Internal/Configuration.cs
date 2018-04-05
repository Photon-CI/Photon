using Photon.Library;
using System;
using System.IO;
using System.Reflection;

namespace Photon.Server.Internal
{
    internal class Configuration
    {
        private static readonly string DefaultDirectory;
        //private static readonly string DefaultDefinitionFile;

        //private static readonly string DefaultProjectDirectory;
        //private static readonly string DefaultWorkDirectory;
        //private static readonly string DefaultProjectPackageDirectory;
        //private static readonly string DefaultApplicationDataDirectory;
        //private static readonly string DefaultApplicationPackageDirectory;

        public static string AssemblyPath {get;}

        public static string Directory => ConfigurationReader.AppSetting("directory", DefaultDirectory);
        public static string DefinitionFile => Path.Combine(Directory, ConfigurationReader.AppSetting("definition", "server.json"));
        public static string ProjectsFile => Path.Combine(Directory, ConfigurationReader.AppSetting("projects", "projects.json"));
        public static string WorkPath => Path.Combine(Directory, ConfigurationReader.AppSetting("work", "Work"));
        public static string ProjectDataPath => Path.Combine(Directory, ConfigurationReader.AppSetting("projectData", "ProjectData"));

        //public static string ProjectPackageDirectory => ConfigurationReader.AppSetting("directory.projectPackages", DefaultProjectPackageDirectory);
        //public static string ApplicationPackageDirectory => ConfigurationReader.AppSetting("directory.applicationPackages", DefaultApplicationPackageDirectory);


        static Configuration()
        {
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyPath = Path.GetDirectoryName(assembly.Location);

            var _appData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            DefaultDirectory = Path.Combine("Photon", "Server");

            //DefaultProjectDirectory = Path.Combine(AssemblyPath, "Projects");
            //DefaultWorkDirectory = Path.Combine(AssemblyPath, "Work");
            //DefaultProjectPackageDirectory = Path.Combine(AssemblyPath, "Projects", "Packages");
            //DefaultApplicationDataDirectory = Path.Combine(AssemblyPath, "Applications", "Data");
            //DefaultApplicationPackageDirectory = Path.Combine(AssemblyPath, "Applications", "Packages");
        }
    }
}
