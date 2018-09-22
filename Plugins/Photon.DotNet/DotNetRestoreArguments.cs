using System.Collections.Generic;

namespace Photon.DotNetPlugin
{
    /// <summary>
    /// Restores the dependencies and tools of a project.
    /// </summary>
    public class DotNetRestoreArguments
    {
        /// <summary>
        /// Optional path to the project file to restore.
        /// </summary>
        public string RootPath {get; set;}

        /// <summary>
        /// The NuGet configuration file (NuGet.config) to use for the restore operation.
        /// </summary>
        public string ConfigurationFile {get; set;}

        /// <summary>
        /// Disables restoring multiple projects in parallel.
        /// </summary>
        public bool DisableParallel {get; set;}

        /// <summary>
        /// Forces all dependencies to be resolved even if the last restore was successful.
        /// Specifying this flag is the same as deleting the project.assets.json file.
        /// </summary>
        public bool Force {get; set;}

        /// <summary>
        /// Only warn about failed sources if there are packages meeting the version requirement.
        /// </summary>
        public bool IgnoreFailedSources {get; set;}

        /// <summary>
        /// Specifies to not cache packages and HTTP requests.
        /// </summary>
        public bool NoCache {get; set;}

        /// <summary>
        /// When restoring a project with project-to-project (P2P) references,
        /// restores the root project and not the references.
        /// </summary>
        public bool NoDependencies {get; set;}

        /// <summary>
        /// Specifies the directory for restored packages.
        /// </summary>
        public string PackagesDirectory {get; set;}

        /// <summary>
        /// Specifies a runtime for the package restore. This is used to restore packages for
        /// runtimes not explicitly listed in the &lt;RuntimeIdentifiers&gt; tag in the .csproj file.
        /// For a list of Runtime Identifiers (RIDs), see the RID catalog.
        /// </summary>
        public List<string> RuntimeIdentifiers {get; set;}

        /// <summary>
        /// Specifies a NuGet package source to use during the restore operation.
        /// This setting overrides all of the sources specified in the NuGet.config files.
        /// </summary>
        public List<string> Sources {get; set;}

        /// <summary>
        /// Sets the verbosity level of the command.
        /// </summary>
        public DotNetVerbosityLevel Verbosity {get; set;}

        /// <summary>
        /// Allows additional undefined arguments to be provided.
        /// </summary>
        public List<string> AdditionalArguments {get; set;}


        public DotNetRestoreArguments()
        {
            RuntimeIdentifiers = new List<string>();
            Sources = new List<string>();
            AdditionalArguments = new List<string>();
            Verbosity = DotNetVerbosityLevel.Normal;
        }

        /// <summary>
        /// Returns the collection of argument strings defined by this instance.
        /// </summary>
        public IEnumerable<string> GetArguments()
        {
            yield return "restore";

            if (!string.IsNullOrEmpty(RootPath))
                yield return $"\"{RootPath}\"";

            if (!string.IsNullOrEmpty(ConfigurationFile))
                yield return $"--configfile \"{ConfigurationFile}\"";

            if (DisableParallel) yield return "--disable-parallel";

            if (Force) yield return "--force";

            if (IgnoreFailedSources) yield return "--ignore-failed-sources";

            if (NoCache) yield return "--no-cache";

            if (NoDependencies) yield return "--no-dependencies";

            if (!string.IsNullOrEmpty(PackagesDirectory))
                yield return $"--packages \"{PackagesDirectory}\"";

            if (RuntimeIdentifiers != null) {
                foreach (var runtime in RuntimeIdentifiers)
                    yield return $"--runtime \"{runtime}\"";
            }

            if (Sources != null) {
                foreach (var source in Sources)
                    yield return $"--source \"{source}\"";
            }

            if (Verbosity != DotNetVerbosityLevel.Normal)
                yield return $"--verbosity {DotNetVerbosity.GetString(Verbosity)}";

            foreach (var arg in AdditionalArguments)
                yield return arg;
        }
    }
}
