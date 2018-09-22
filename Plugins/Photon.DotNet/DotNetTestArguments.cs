using System.Collections.Generic;

namespace Photon.DotNetPlugin
{
    /// <summary>
    /// .NET test driver used to execute unit tests.
    /// </summary>
    public class DotNetTestArguments
    {
        /// <summary>
        /// Path to the test project. If not specified, it defaults to current directory.
        /// </summary>
        public string ProjectFile {get; set;}

        /// <summary>
        /// Use the custom test adapters from the specified path in the test run.
        /// </summary>
        public string TestAdapterPath {get; set;}

        /// <summary>
        /// Runs the tests in blame mode. This option is helpful in isolating the problematic
        /// tests causing test host to crash. It creates an output file in the current directory
        /// as Sequence.xml that captures the order of tests execution before the crash.
        /// </summary>
        public bool Blame {get; set;}

        /// <summary>
        /// Defines the build configuration. The default value is Debug, but your
        /// project's configuration could override this default SDK setting.
        /// </summary>
        public string Configuration {get; set;}

        /// <summary>
        /// Enables data collector for the test run.
        /// </summary>
        public string CollectorName {get; set;}

        /// <summary>
        /// Enables diagnostic mode for the test platform and
        /// write diagnostic messages to the specified file.
        /// </summary>
        public string DiagnosticsFile {get; set;}

        /// <summary>
        /// Looks for test binaries for a specific framework.
        /// </summary>
        public string Framework {get; set;}

        /// <summary>
        /// Filters out tests in the current project using the given expression.
        /// </summary>
        public string Filter {get; set;}

        /// <summary>
        /// Specifies a logger Uri or friendly-name for test results.
        /// </summary>
        public string Logger {get; set;}

        /// <summary>
        /// Doesn't build the test project before running it. It also implicit sets the NoRestore flag.
        /// </summary>
        public bool NoBuild {get; set;}

        /// <summary>
        /// Doesn't execute an implicit restore when running the command.
        /// </summary>
        public bool NoRestore {get; set;}

        /// <summary>
        /// Directory in which to find the binaries to run.
        /// </summary>
        public string OutputDirectory {get; set;}

        /// <summary>
        /// The directory where the test results are going to be placed.
        /// If the specified directory doesn't exist, it's created.
        /// </summary>
        public string ResultsDirectory {get; set;}

        /// <summary>
        /// Settings to use when running tests.
        /// </summary>
        public string SettingsFile {get; set;}

        /// <summary>
        /// List all of the discovered tests in the current project.
        /// </summary>
        public bool ListTests {get; set;}

        /// <summary>
        /// Sets the verbosity level of the command.
        /// </summary>
        public DotNetVerbosityLevel Verbosity {get; set;}

        /// <summary>
        /// Allows additional undefined arguments to be provided.
        /// </summary>
        public List<string> AdditionalArguments {get; set;}


        public DotNetTestArguments()
        {
            AdditionalArguments = new List<string>();
            Verbosity = DotNetVerbosityLevel.Normal;
        }

        /// <summary>
        /// Returns the collection of argument strings defined by this instance.
        /// </summary>
        public IEnumerable<string> GetArguments()
        {
            yield return "test";

            if (!string.IsNullOrEmpty(ProjectFile))
                yield return $"\"{ProjectFile}\"";

            if (!string.IsNullOrEmpty(TestAdapterPath))
                yield return $"--test-adapter-path \"{TestAdapterPath}\"";

            if (Blame) yield return "--blame";

            if (!string.IsNullOrEmpty(Configuration))
                yield return $"--configuration \"{Configuration}\"";

            if (!string.IsNullOrEmpty(CollectorName))
                yield return $"--collect \"{CollectorName}\"";

            if (!string.IsNullOrEmpty(DiagnosticsFile))
                yield return $"--diag \"{DiagnosticsFile}\"";

            if (!string.IsNullOrEmpty(Framework))
                yield return $"--framework \"{Framework}\"";

            if (!string.IsNullOrEmpty(Filter))
                yield return $"--filter \"{Filter}\"";

            if (!string.IsNullOrEmpty(Logger))
                yield return $"--logger \"{Logger}\"";

            if (NoBuild) yield return "--no-build";

            if (NoRestore) yield return "--no-restore";

            if (!string.IsNullOrEmpty(OutputDirectory))
                yield return $"--output \"{OutputDirectory}\"";

            if (!string.IsNullOrEmpty(ResultsDirectory))
                yield return $"--results-directory \"{ResultsDirectory}\"";

            if (!string.IsNullOrEmpty(SettingsFile))
                yield return $"--settings \"{SettingsFile}\"";

            if (ListTests) yield return "--list-tests";

            if (Verbosity != DotNetVerbosityLevel.Normal)
                yield return $"--verbosity {DotNetVerbosity.GetString(Verbosity)}";

            foreach (var arg in AdditionalArguments)
                yield return arg;
        }
    }
}
