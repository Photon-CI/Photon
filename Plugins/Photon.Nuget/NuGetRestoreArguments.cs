using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.NuGetPlugin
{
    /// <summary>
    /// Downloads and installs any packages missing from the packages folder.
    /// When used with NuGet 4.0+ and the PackageReference format, generates a
    /// &lt;project&gt;.nuget.props file, if needed, in the obj folder.
    /// (The file can be omitted from source control.)
    /// </summary>
    public class NuGetRestoreArguments
    {
        /// <summary>
        /// Specifies the location of a solution or a packages.config file.
        /// </summary>
        public string ProjectPath {get; set;}

        /// <summary>
        /// The NuGet configuration file to apply. If not specified, %AppData%\NuGet\NuGet.Config
        /// (Windows) or ~/.nuget/NuGet/NuGet.Config (Mac/Linux) is used.
        /// </summary>
        public string ConfigFile {get; set;}

        /// <summary>
        /// (4.0+) Downloads packages directly without populating caches with any binaries or metadata.
        /// </summary>
        public bool DirectDownload {get; set;}

        /// <summary>
        /// Disables restoring multiple packages in parallel.
        /// </summary>
        public bool DisableParallelProcessing {get; set;}

        /// <summary>
        /// (3.2+) A list of package sources to use as fallbacks in case the package isn't
        /// found in the primary or default source.
        /// </summary>
        public List<string> FallbackSources {get; set;}

        /// <summary>
        /// (3.5+) Forces nuget.exe to run using an invariant, English-based culture.
        /// </summary>
        public bool ForceEnglishOutput {get; set;}

        /// <summary>
        /// (4.0+) Specifies the path of MSBuild to use with the command,
        /// taking precedence over -MSBuildVersion.
        /// </summary>
        public string MSBuildPath {get; set;}

        /// <summary>
        /// (3.2+) Specifies the version of MSBuild to be used with this command. Supported
        /// values are 4, 12, 14, 15. By default the MSBuild in your path is picked, otherwise
        /// it defaults to the highest installed version of MSBuild.
        /// </summary>
        public string MSBuildVersion {get; set;}

        /// <summary>
        /// Prevents NuGet from using cached packages.
        /// </summary>
        public bool NoCache {get; set;}

        /// <summary>
        /// Suppresses prompts for user input or confirmations.
        /// </summary>
        public bool NonInteractive {get; set;}

        /// <summary>
        /// Specifies the folder in which packages are installed. If no folder is specified,
        /// the current folder is used. Required when restoring with a packages.config file
        /// unless PackagesDirectory or SolutionDirectory is used.
        /// </summary>
        public string OutputDirectory {get; set;}

        /// <summary>
        /// Specifies the types of files to save after package installation.
        /// </summary>
        public PackageSaveModes PackageSaveMode {get; set;}

        /// <summary>
        /// Same as OutputDirectory. Required when restoring with a packages.config
        /// file unless OutputDirectory or SolutionDirectory is used.
        /// </summary>
        public string PackagesDirectory {get; set;}

        /// <summary>
        /// Timeout in seconds for resolving project-to-project references.
        /// </summary>
        public int Project2ProjectTimeOut {get; set;}

        /// <summary>
        /// (4.0+) Restores all references projects for UWP and .NET Core projects.
        /// Does not apply to projects using packages.config.
        /// </summary>
        public bool Recursive {get; set;}

        /// <summary>
        /// Verifies that restoring packages is enabled before
        /// downloading and installing  the packages.
        /// </summary>
        public bool RequireConsent {get; set;}

        /// <summary>
        /// Specifies the solution folder. Not valid when restoring packages for
        /// a solution. Required when restoring with a packages.config file unless
        /// PackagesDirectory or OutputDirectory is used.
        /// </summary>
        public string SolutionDirectory {get; set;}

        /// <summary>
        /// Specifies the list of package sources (as URLs) to use for the restore.
        /// If omitted, the command uses the sources provided in configuration files.
        /// </summary>
        public List<string> Sources {get; set;}

        /// <summary>
        /// Specifies the amount of detail displayed in the output.
        /// </summary>
        public NuGetVerbosity Verbosity {get; set;}

        /// <summary>
        /// Allows additional undefined arguments to be provided.
        /// </summary>
        public List<string> AdditionalArguments {get; set;}


        public NuGetRestoreArguments()
        {
            PackageSaveMode = PackageSaveModes.Nupkg;
            FallbackSources = new List<string>();
            Sources = new List<string>();
            Verbosity = NuGetVerbosity.Normal;
            AdditionalArguments = new List<string>();
        }

        /// <summary>
        /// Returns the collection of argument strings defined by this instance.
        /// </summary>
        public IEnumerable<string> GetArguments()
        {
            if (string.IsNullOrEmpty(ProjectPath)) throw new ArgumentNullException(nameof(ProjectPath), "ProjectPath must be specified!");

            yield return "restore";

            yield return $"\"{ProjectPath}\"";

            if (!string.IsNullOrEmpty(ConfigFile))
                yield return $"-ConfigFile \"{ConfigFile}\"";

            if (DirectDownload)
                yield return "-DirectDownload";

            if (DisableParallelProcessing)
                yield return "-DisableParallelProcessing";

            if (FallbackSources?.Any() ?? false)
                yield return $"-FallbackSource \"{string.Join(";", FallbackSources)}\"";

            if (ForceEnglishOutput)
                yield return "-ForceEnglishOutput";

            if (!string.IsNullOrEmpty(MSBuildPath))
                yield return $"-MSBuildPath \"{MSBuildPath}\"";

            if (!string.IsNullOrEmpty(MSBuildVersion))
                yield return $"-MSBuildVersion \"{MSBuildVersion}\"";

            if (NoCache)
                yield return "-NoCache";

            if (NonInteractive)
                yield return "-NonInteractive";

            if (!string.IsNullOrEmpty(OutputDirectory))
                yield return $"-OutputDirectory \"{OutputDirectory}\"";

            if (PackageSaveMode != PackageSaveModes.Nupkg)
                yield return $"-PackageSaveMode \"{GetPackageSaveMode()}\"";

            if (!string.IsNullOrEmpty(PackagesDirectory))
                yield return $"-PackagesDirectory \"{PackagesDirectory}\"";

            if (Project2ProjectTimeOut > 0)
                yield return $"-Project2ProjectTimeOut {Project2ProjectTimeOut}";

            if (Recursive)
                yield return "-Recursive";

            if (RequireConsent)
                yield return "-RequireConsent";

            if (!string.IsNullOrEmpty(SolutionDirectory))
                yield return $"-SolutionDirectory \"{SolutionDirectory}\"";

            if (Sources?.Any() ?? false)
                yield return $"-Source \"{string.Join(";", Sources)}\"";

            if (Verbosity != NuGetVerbosity.Normal)
                yield return $"-Verbosity \"{GetVerbosityString()}\"";

            foreach (var arg in AdditionalArguments)
                yield return arg;
        }

        private string GetVerbosityString()
        {
            switch (Verbosity) {
                case NuGetVerbosity.Detailed: return "detailed";
                case NuGetVerbosity.Quiet: return "quiet";
                default:
                case NuGetVerbosity.Normal: return "normal";
            }
        }

        private string GetPackageSaveMode()
        {
            switch (PackageSaveMode) {
                case PackageSaveModes.Nuspec: return "nuspec";
                case PackageSaveModes.Both: return "nuspec;nupkg";
                default:
                case PackageSaveModes.Nupkg: return "nupkg";
            }
        }
    }
}
