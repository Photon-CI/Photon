using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.NuGetPlugin
{
    /// <summary>
    /// Creates a NuGet package based on the specified .nuspec or project file.
    /// </summary>
    public class NuGetPackArguments
    {
        /// <summary>
        /// Gets or sets the filename of the nuspec or project file to package.
        /// </summary>
        public string Filename {get; set;}

        /// <summary>
        /// Sets the base path of the files defined in the .nuspec file.
        /// </summary>
        public string BasePath {get; set;}

        /// <summary>
        /// Specifies that the project should be built before building the package.
        /// </summary>
        public bool Build {get; set;}

        /// <summary>
        /// Specifies one or more wildcard patterns to exclude when creating a package.
        /// </summary>
        public List<string> Exclude {get; set;}

        /// <summary>
        /// Prevents inclusion of empty directories when building the package.
        /// </summary>
        public bool ExcludeEmptyDirectories {get; set;}

        /// <summary>
        /// (3.5+) Forces nuget.exe to run using an invariant, English-based culture.
        /// </summary>
        public bool ForceEnglishOutput {get; set;}

        /// <summary>
        /// Indicates that the built package should include referenced projects either as
        /// dependencies or as part of the package. If a referenced project has a corresponding
        /// .nuspec file that has the same name as the project, then that referenced project is
        /// added as a dependency. Otherwise, the referenced project is added as part of the package.
        /// </summary>
        public bool IncludeReferencedProjects {get; set;}

        /// <summary>
        /// Set the minClientVersion attribute for the created package. This value will override
        /// the value of the existing minClientVersion attribute (if any) in the .nuspec file.
        /// </summary>
        public string MinClientVersion {get; set;}

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
        /// Prevents default exclusion of NuGet package files and files and folders
        /// starting with a dot, such as .svn and .gitignore.
        /// </summary>
        public bool NoDefaultExcludes {get; set;}

        /// <summary>
        /// Specifies that pack should not run package analysis after building the package.
        /// </summary>
        public bool NoPackageAnalysis {get; set;}

        /// <summary>
        /// Specifies the folder in which the created package is stored.
        /// If no folder is specified, the current folder is used.
        /// </summary>
        public string OutputDirectory {get; set;}

        /// <summary>
        /// Specifies a list of properties that override values in the project file; see
        /// Common MSBuild Project Properties for property names. Each occurrence of $token$
        /// in the .nuspec file will be replaced with the given value.
        /// </summary>
        public IDictionary<string, string> Properties {get; set;}

        /// <summary>
        /// (3.4.4+) Appends a suffix to the internally generated version number, typically
        /// used for appending build or other pre-release identifiers. For example, using -suffix
        /// nightly will create a package with a version number like 1.2.3-nightly. Suffixes
        /// must start with a letter to avoid warnings, errors, and potential incompatibilities
        /// with different versions of NuGet and the NuGet Package Manager.
        /// </summary>
        public string Suffix {get; set;}

        /// <summary>
        /// Specifies that the package contains sources and symbols. When used with a .nuspec
        /// file, this creates a regular NuGet package file and the corresponding symbols package.
        /// </summary>
        public bool Symbols {get; set;}

        /// <summary>
        /// Specifies that the output files of the project should be placed in the tool folder.
        /// </summary>
        public bool Tool {get; set;}

        /// <summary>
        /// Specifies the amount of detail displayed in the output.
        /// </summary>
        public NuGetVerbosity Verbosity {get; set;}

        /// <summary>
        /// Overrides the version number from the .nuspec file.
        /// </summary>
        public string Version {get; set;}

        /// <summary>
        /// Allows additional undefined arguments to be provided.
        /// </summary>
        public List<string> AdditionalArguments {get; set;}


        public NuGetPackArguments()
        {
            Exclude = new List<string>();
            Properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            AdditionalArguments = new List<string>();
            Verbosity = NuGetVerbosity.Normal;
        }

        /// <summary>
        /// Returns the collection of argument strings defined by this instance.
        /// </summary>
        public IEnumerable<string> GetArguments()
        {
            if (string.IsNullOrEmpty(Filename)) throw new ArgumentNullException(nameof(Filename), "Filename must be specified!");

            yield return "pack";

            yield return $"\"{Filename}\"";

            if (!string.IsNullOrEmpty(BasePath))
                yield return $"-BasePath \"{BasePath}\"";

            if (Build)
                yield return "-Build";

            foreach (var exclude in Exclude)
                yield return $"-Exclude \"{exclude}\"";

            if (ExcludeEmptyDirectories)
                yield return "-ExcludeEmptyDirectories";

            if (ForceEnglishOutput)
                yield return "-ForceEnglishOutput";

            if (IncludeReferencedProjects)
                yield return "-IncludeReferencedProjects";

            if (!string.IsNullOrEmpty(MinClientVersion))
                yield return $"-MinClientVersion \"{MinClientVersion}\"";

            if (!string.IsNullOrEmpty(MSBuildPath))
                yield return $"-MSBuildPath \"{MSBuildPath}\"";

            if (!string.IsNullOrEmpty(MSBuildVersion))
                yield return $"-MSBuildVersion \"{MSBuildVersion}\"";

            if (NoDefaultExcludes)
                yield return "-NoDefaultExcludes";

            if (NoPackageAnalysis)
                yield return "-NoPackageAnalysis";

            if (!string.IsNullOrEmpty(OutputDirectory))
                yield return $"-OutputDirectory \"{OutputDirectory}\"";

            if (!string.IsNullOrEmpty(Suffix))
                yield return $"-Suffix \"{Suffix}\"";

            if (Symbols)
                yield return "-Symbols";

            if (Tool)
                yield return "-Tool";

            if (Verbosity != NuGetVerbosity.Normal)
                yield return $"-Verbosity \"{GetVerbosityString()}\"";

            if (!string.IsNullOrEmpty(Version))
                yield return $"-Version \"{Version}\"";

            foreach (var arg in AdditionalArguments)
                yield return arg;

            if (Properties?.Any() ?? false) {
                var p = Properties.Select(x => $"{x.Key}=\"{x.Value}\"");
                yield return $"-Properties {string.Join(";", p)}";
            }
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
    }
}
