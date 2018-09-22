using System.Collections.Generic;

namespace Photon.DotNetPlugin
{
    /// <summary>
    /// Packs the code into a NuGet package.
    /// </summary>
    public class DotNetPackArguments
    {
        /// <summary>
        /// The project to pack. It's either a path to a csproj file or to a directory.
        /// If not specified, it defaults to the current directory.
        /// </summary>
        public string ProjectFile {get; set;}

        /// <summary>
        /// Defines the build configuration. The default value is Debug.
        /// </summary>
        public string Configuration {get; set;}

        /// <summary>
        /// Forces all dependencies to be resolved even if the last restore was successful.
        /// Specifying this flag is the same as deleting the project.assets.json file.
        /// </summary>
        public bool Force {get; set;}

        /// <summary>
        /// Includes the source files in the NuGet package. The sources files
        /// are included in the src folder within the nupkg.
        /// </summary>
        public bool IncludeSource {get; set;}

        /// <summary>
        /// Generates the symbols nupkg.
        /// </summary>
        public bool IncludeSymbols {get; set;}

        /// <summary>
        /// Doesn't build the project before packing.
        /// It also implicit sets the NoRestore flag.
        /// </summary>
        public bool NoBuild {get; set;}

        /// <summary>
        /// Ignores project-to-project references and only restores the root project.
        /// </summary>
        public bool NoDependencies {get; set;}

        /// <summary>
        /// Doesn't execute an implicit restore when running the command.
        /// </summary>
        public bool NoRestore {get; set;}

        public string OutputDirectory {get; set;}

        /// <summary>
        /// Specifies the target runtime to restore packages for.
        /// </summary>
        public string RuntimeIdentifier {get; set;}

        /// <summary>
        /// Sets the serviceable flag in the package.
        /// </summary>
        public bool Serviceable {get; set;}

        /// <summary>
        /// Defines the value for the $(VersionSuffix)
        /// MSBuild property in the project.
        /// </summary>
        public string VersionSuffix {get; set;}

        /// <summary>
        /// Sets the verbosity level of the command.
        /// </summary>
        public DotNetVerbosityLevel Verbosity {get; set;}

        /// <summary>
        /// Allows additional undefined arguments to be provided.
        /// </summary>
        public List<string> AdditionalArguments {get; set;}


        public DotNetPackArguments()
        {
            AdditionalArguments = new List<string>();
            Verbosity = DotNetVerbosityLevel.Normal;
        }

        /// <summary>
        /// Returns the collection of argument strings defined by this instance.
        /// </summary>
        public IEnumerable<string> GetArguments()
        {
            yield return "pack";

            if (!string.IsNullOrEmpty(ProjectFile))
                yield return $"\"{ProjectFile}\"";

            if (!string.IsNullOrEmpty(Configuration))
                yield return $"--configuration \"{Configuration}\"";

            if (Force) yield return "--force";

            if (IncludeSource) yield return "--include-source";

            if (IncludeSymbols) yield return "--include-symbols";

            if (NoBuild) yield return "--no-build";

            if (NoDependencies) yield return "--no-dependencies";

            if (NoRestore) yield return "--no-restore";

            if (!string.IsNullOrEmpty(OutputDirectory))
                yield return $"--output \"{OutputDirectory}\"";

            if (!string.IsNullOrEmpty(RuntimeIdentifier))
                yield return $"--runtime \"{RuntimeIdentifier}\"";

            if (Serviceable) yield return "--serviceable";

            if (!string.IsNullOrEmpty(VersionSuffix))
                yield return $"--version-suffix \"{VersionSuffix}\"";

            if (Verbosity != DotNetVerbosityLevel.Normal)
                yield return $"--verbosity {DotNetVerbosity.GetString(Verbosity)}";

            foreach (var arg in AdditionalArguments)
                yield return arg;
        }
    }
}
