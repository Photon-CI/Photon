using System.Collections.Generic;

namespace Photon.DotNetPlugin
{
    /// <summary>
    /// Builds a project and all of its dependencies.
    /// </summary>
    public class DotNetBuildArguments
    {
        /// <summary>
        /// The project file to build. If a project file is not specified,
        /// MSBuild searches the current working directory for a file that
        /// has a file extension that ends in proj and uses that file.
        /// </summary>
        public string ProjectFile {get; set;}

        /// <summary>
        /// Defines the build configuration. The default value is Debug.
        /// </summary>
        public string Configuration {get; set;}

        /// <summary>
        /// Compiles for a specific framework. The framework must
        /// be defined in the project file.
        /// </summary>
        public string Framework {get; set;}

        /// <summary>
        /// Forces all dependencies to be resolved even if the last restore was successful.
        /// Specifying this flag is the same as deleting the project.assets.json file.
        /// </summary>
        public bool Force {get; set;}

        /// <summary>
        /// Ignores project-to-project (P2P) references and only builds the specified root project.
        /// </summary>
        public bool NoDependencies {get; set;}

        /// <summary>
        /// Marks the build as unsafe for incremental build. This flag turns off incremental
        /// compilation and forces a clean rebuild of the project's dependency graph.
        /// </summary>
        public bool NoIncremental {get; set;}

        /// <summary>
        /// Doesn't execute an implicit restore during build.
        /// </summary>
        public bool NoRestore {get; set;}

        /// <summary>
        /// Directory in which to place the built binaries. You also
        /// need to define --framework when you specify this option.
        /// </summary>
        public string Output {get; set;}

        /// <summary>
        /// Specifies the target runtime. For a list of
        /// Runtime Identifiers (RIDs), see the RID catalog.
        /// </summary>
        public string RuntimeIdentifier {get; set;}

        /// <summary>
        /// Sets the verbosity level of the command.
        /// </summary>
        public DotNetVerbosityLevel Verbosity {get; set;}

        /// <summary>
        /// Defines the version suffix for an asterisk (*) in the
        /// version field of the project file. The format follows
        /// NuGet's version guidelines.
        /// </summary>
        public string VersionSuffix {get; set;}

        /// <summary>
        /// Allows additional undefined arguments to be provided.
        /// </summary>
        public List<string> AdditionalArguments {get; set;}


        public DotNetBuildArguments()
        {
            AdditionalArguments = new List<string>();
            Verbosity = DotNetVerbosityLevel.Normal;
        }

        /// <summary>
        /// Returns the collection of argument strings defined by this instance.
        /// </summary>
        public IEnumerable<string> GetArguments()
        {
            yield return "build";

            if (!string.IsNullOrEmpty(ProjectFile))
                yield return $"\"{ProjectFile}\"";

            if (!string.IsNullOrEmpty(Configuration))
                yield return $"--configuration \"{Configuration}\"";

            if (!string.IsNullOrEmpty(Framework))
                yield return $"--framework \"{Framework}\"";

            if (Force) yield return "--force";

            if (NoDependencies) yield return "--no-dependencies";

            if (NoIncremental) yield return "--no-incremental";

            if (NoRestore) yield return "--no-restore";

            if (!string.IsNullOrEmpty(Output))
                yield return $"--output \"{Output}\"";

            if (!string.IsNullOrEmpty(RuntimeIdentifier))
                yield return $"--runtime  \"{RuntimeIdentifier}\"";

            if (Verbosity != DotNetVerbosityLevel.Normal)
                yield return $"--verbosity {DotNetVerbosity.GetString(Verbosity)}";

            if (!string.IsNullOrEmpty(VersionSuffix))
                yield return $"--version-suffix \"{VersionSuffix}\"";

            foreach (var arg in AdditionalArguments)
                yield return arg;
        }
    }
}
