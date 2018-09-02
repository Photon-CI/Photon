using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.MSBuild
{
    /// <summary>
    /// Defines the collection of arguments that can be passed to MSBuild on the command line.
    /// </summary>
    public class MSBuildArguments
    {
        /// <summary>
        /// Builds the targets in the project file that you specify. If you don't specify a
        /// project file, MSBuild searches the current working directory for a file name
        /// extension that ends in "proj" and uses that file. You can also specify a Visual
        /// Studio solution file for this argument.
        /// </summary>
        public string ProjectFile {get; set;}

        /// <summary>
        /// Gets or sets whether to show detailed information at the end of the build log about
        /// the configurations that were built and how they were scheduled to nodes.
        /// </summary>
        public bool DetailedSummary {get; set;}

        /// <summary>
        /// Ignore the specified extensions when determining which project file to build. Use
        /// a semicolon or a comma to separate multiple extensions.
        /// </summary>
        public List<string> IgnoreProjectExtensions {get; set;}

        /// <summary>
        /// Specifies the maximum number of concurrent processes to use when building. The default
        /// value is 1. Setting this value to 0 will use up to the number of processors in the computer.
        /// </summary>
        public int MaxCpuCount {get; set;}

        /// <summary>
        /// Don't include any MSBuild.rsp files automatically.
        /// </summary>
        public bool NoAutoResponse {get; set;}

        /// <summary>
        /// Enable or disable the re-use of MSBuild nodes. A node corresponds to a project that's
        /// executing. If you include the /maxcpucount switch, multiple nodes can execute concurrently.
        /// </summary>
        public bool NodeReuse {get; set;}

        /// <summary>
        /// Don't display the startup banner or the copyright message.
        /// </summary>
        public bool NoLogo {get; set;}

        /// <summary>
        /// Create a single, aggregated project file by inlining all the files that would be imported
        /// during a build, with their boundaries marked. You can use this switch to more easily
        /// determine which files are being imported, from where the files are being imported, and
        /// which files contribute to the build. When you use this switch, the project isn't built.
        /// </summary>
        public string PreProcess {get; set;}

        /// <summary>
        /// Gets or sets a list of overrides for the specified project-level properties.
        /// </summary>
        public Dictionary<string, string> Properties {get; set;}

        /// <summary>
        /// Build the specified targets in the project. If you specify any targets by using this switch,
        /// they are run instead of any targets in the DefaultTargets attribute in the project file.
        /// </summary>
        public List<string> Targets {get; set;}

        /// <summary>
        /// Specifies the version of the Toolset to use to build the project. By using this switch, you
        /// can build a project and specify a version that differs from the version that's specified in
        /// the Project Element (MSBuild).
        /// </summary>
        public string ToolsVersion {get; set;}

        /// <summary>
        /// Validate the project file and, if validation succeeds, build the project. If you don't specify
        /// schema, the project is validated against the default schema.
        /// </summary>
        public string Validate {get; set;}

        /// <summary>
        /// Specifies the amount of information to display in the build log. Each logger displays events
        /// based on the verbosity level that you set for that logger.
        /// </summary>
        public MSBuildVerbosityLevel Verbosity {get; set;}

        /// <summary>
        /// Display version information only. The project isn't built.
        /// </summary>
        public bool Version {get; set;}


        public MSBuildArguments()
        {
            MaxCpuCount = 1;
            NodeReuse = true;
            Verbosity = MSBuildVerbosityLevel.Normal;
            IgnoreProjectExtensions = new List<string>();
            Properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Targets = new List<string>();
        }

        /// <summary>
        /// Returns the collection of argument strings defined by this instance.
        /// </summary>
        public IEnumerable<string> GetArguments()
        {
            if (DetailedSummary)
                yield return "/ds";

            if (IgnoreProjectExtensions?.Any() ?? false)
                yield return $"/ignore:{string.Join(";", IgnoreProjectExtensions)}";

            if (MaxCpuCount == 0)
                yield return "/m";

            if (MaxCpuCount > 1)
                yield return $"/m:{MaxCpuCount}";

            if (NoAutoResponse)
                yield return "/noautorsp";

            if (!NodeReuse)
                yield return "/nr:false";

            if (NoLogo)
                yield return "/nologo";

            if (!string.IsNullOrEmpty(PreProcess))
                yield return $"/pp:\"{PreProcess}\"";

            foreach (var property in Properties)
                yield return $"/p:{property.Key}=\"{property.Value}\"";

            foreach (var target in Targets)
                yield return $"/t:\"{target}\"";

            if (!string.IsNullOrEmpty(ToolsVersion))
                yield return $"/tv:\"{ToolsVersion}\"";

            if (!string.IsNullOrEmpty(Validate))
                yield return $"/val:\"{Validate}\"";

            if (Verbosity != MSBuildVerbosityLevel.Normal)
                yield return $"/v:{GetVerbosityString()}";

            if (Version)
                yield return "/ver";

            if (!string.IsNullOrEmpty(ProjectFile))
                yield return $"\"{ProjectFile}\"";
        }

        private string GetVerbosityString()
        {
            switch (Verbosity) {
                case MSBuildVerbosityLevel.Quiet: return "q";
                case MSBuildVerbosityLevel.Minimal: return "m";
                case MSBuildVerbosityLevel.Detailed: return "d";
                case MSBuildVerbosityLevel.Diagnostic: return "diag";
                default:
                case MSBuildVerbosityLevel.Normal: return "n";
            }
        }
    }

    public enum MSBuildVerbosityLevel
    {
        Quiet,
        Minimal,
        Normal,
        Detailed,
        Diagnostic,
    }
}
