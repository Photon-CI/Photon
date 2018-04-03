using Photon.Library;
using System.IO;
using System.Reflection;

namespace Photon.Server.Internal
{
    internal class Configuration
    {
        private static readonly string DefaultDirectory;

        private static readonly string DefaultProjectDirectory;
        private static readonly string DefaultWorkDirectory;
        private static readonly string DefaultProjectPackageDirectory;
        private static readonly string DefaultApplicationDataDirectory;
        private static readonly string DefaultApplicationPackageDirectory;

        public static string AssemblyPath {get;}

        public static string Directory => ConfigurationReader.AppSetting("directory");
        public static string DefinitionPath => ConfigurationReader.AppSetting("definition");

        public static string ProjectDirectory => ConfigurationReader.AppSetting("directory.projects", DefaultProjectDirectory);
        public static string WorkDirectory => ConfigurationReader.AppSetting("directory.work", DefaultWorkDirectory);
        public static string ProjectPackageDirectory => ConfigurationReader.AppSetting("directory.projectPackages", DefaultProjectPackageDirectory);
        public static string ApplicationDataDirectory => ConfigurationReader.AppSetting("directory.applicationData", DefaultApplicationDataDirectory);
        public static string ApplicationPackageDirectory => ConfigurationReader.AppSetting("directory.applicationPackages", DefaultApplicationPackageDirectory);


        static Configuration()
        {
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyPath = Path.GetDirectoryName(assembly.Location);

            DefaultProjectDirectory = Path.Combine(AssemblyPath, "Projects");
            DefaultWorkDirectory = Path.Combine(AssemblyPath, "Work");
            DefaultProjectPackageDirectory = Path.Combine(AssemblyPath, "Projects", "Packages");
            DefaultApplicationDataDirectory = Path.Combine(AssemblyPath, "Applications", "Data");
            DefaultApplicationPackageDirectory = Path.Combine(AssemblyPath, "Applications", "Packages");
        }
    }
}
