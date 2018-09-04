using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.NUnitPlugin
{
    /// <summary>
    /// Defines the collection of arguments that can be passed to NUnit on the command line.
    /// </summary>
    public class NUnit3Arguments
    {
        /// <summary>
        /// Gets or sets the collection of assembly files to be tested.
        /// </summary>
        public List<string> InputFiles {get; set;}

        /// <summary>
        /// Specifies the name (or path) of a file containing additional command-line
        /// arguments to be interpolated at the point where the @FILE expression appears.
        /// Each line in the file represents a separate command-line argument. 
        /// </summary>
        public List<string> ArgumentFiles {get; set;}

        /// <summary>
        /// Gets or sets a list of names of tests to run or explore. This option
        /// may be repeated. Note that this option is retained for backward
        /// compatibility. The --where option can now be used instead.
        /// </summary>
        public List<string> TestNames {get; set;}

        /// <summary>
        /// Gets or sets the name (or path) of a file containing a list of
        /// tests to run or explore, one per line.
        /// </summary>
        public string TestListFile {get; set;}

        /// <summary>
        /// An expression indicating which tests to run. It may specify test names,
        /// classes, methods, categories or properties comparing them to actual values
        /// with the operators ==, !=, =~ and !~.
        /// </summary>
        public string Where {get; set;}

        /// <summary>
        /// Gets or sets a collection of test parameters for consumption by tests. Case-sensitive.
        /// </summary>
        public Dictionary<string, string> Parameters {get; set;}

        /// <summary>
        /// Gets or sets the name of a project configuration to load (e.g.: Debug).
        /// </summary>
        public string Configuration {get; set;}

        /// <summary>
        /// Gets or sets the process isolation for test assemblies. If not specified,
        /// defaults to Separate for a single assembly or Multiple for more than one.
        /// By default, processes are run in parallel.
        /// </summary>
        public NUnit3ProcessIsolation ProcessIsolation {get; set;}

        /// <summary>
        /// This option is a synonym for --process=Single
        /// </summary>
        public bool InProcess {get; set;}

        /// <summary>
        /// Gets or sets the number of agents that may be allowed to run simultaneously
        /// assuming you are not running InProcess. If not specified, all agent processes
        /// run tests at the same time, whatever the number of assemblies. This setting
        /// is used to control running your assemblies in parallel.
        /// </summary>
        public int? NumberOfAgents {get; set;}

        /// <summary>
        /// Gets or sets the domain isolation for test assemblies. If not specified,
        /// defaults to Single for a single assembly or Multiple for more than one.
        /// </summary>
        public NUnit3DomainIsolation DomainIsolation {get; set;}

        /// <summary>
        /// Gets or sets the framework type/version to use for tests.
        /// Examples: mono, net-4.5, v4.0, 2.0, mono-4.0
        /// </summary>
        public string Framework {get; set;}

        /// <summary>
        /// Run tests in a 32-bit process on 64-bit systems.
        /// </summary>
        public bool ForceX86 {get; set;}

        /// <summary>
        /// Gets or sets whether to dispose each test runner after it has finished running its tests.
        /// </summary>
        public bool DisposeRunners {get; set;}

        /// <summary>
        /// Gets or sets the timeout for each test case in milliseconds.
        /// </summary>
        public long Timeout {get; set;}

        /// <summary>
        /// Gets or sets the random seed used to generate test cases.
        /// </summary>
        public long Seed {get; set;}

        /// <summary>
        /// Gets or sets the number of worker threads to be used in running tests.
        /// This setting is used to control running your tests in parallel and is
        /// used in conjunction with the Parallelizable Attribute. If not specified,
        /// workers defaults to the number of processors on the machine, or 2,
        /// whichever is greater.
        /// </summary>
        public int Workers {get; set;}

        /// <summary>
        /// Gets or sets whether to stop running immediately upon any test failure or error.
        /// </summary>
        public bool StopOnError {get; set;}

        /// <summary>
        /// Gets or sets whether to skip any non-test assemblies specified, without error.
        /// </summary>
        public bool SkipNonTestAssemblies {get; set;}

        /// <summary>
        /// Causes NUnit to break into the debugger immediately before it executes your tests.
        /// This is particularly useful when the tests are running in a separate process to
        /// which you would otherwise have to attach.
        /// </summary>
        public bool Debug {get; set;}

        /// <summary>
        /// Gets or sets the path of the directory to use for output files.
        /// </summary>
        public string WorkPath {get; set;}

        /// <summary>
        /// Gets or sets the file path to contain text output from the tests.
        /// </summary>
        public string OutputPath {get; set;}

        /// <summary>
        /// Gets or sets a list of output specs for saving the test results.
        /// </summary>
        public List<string> ResultSpecs {get; set;}

        /// <summary>
        /// Gets or sets whether to display or save test info rather than running tests.
        /// Optionally provide an output SPEC for saving the test info. This option may be repeated.
        /// </summary>
        public bool Explore {get; set;}

        /// <summary>
        /// Gets or sets whether to display or save test info rather than running tests.
        /// </summary>
        public List<string> ExploreSpecs {get; set;}

        /// <summary>
        /// Don't save any test results.
        /// </summary>
        public bool NoResult {get; set;}

        /// <summary>
        /// Gets or sets the internal trace level.
        /// </summary>
        public NUnit3TraceLevel TraceLevel {get; set;}

        /// <summary>
        /// Gets or sets whether to write test case names to the output.
        /// </summary>
        public NUnit3Labels Labels {get; set;}

        /// <summary>
        /// Specify a non-standard naming pattern to use when generating all test names.
        /// </summary>
        public string TestNameFormat {get; set;}

        /// <summary>
        /// Specify the Console CodePage, such as utf-8, ascii, etc. This option
        /// is not normally needed unless your output includes special characters.
        /// The page specified must be available on the system.
        /// </summary>
        public string Encoding {get; set;}

        /// <summary>
        /// Tells .NET to copy loaded assemblies to the shadowcopy directory.
        /// </summary>
        public bool ShadowCopy {get; set;}

        /// <summary>
        /// Turns on use of TeamCity service messages.
        /// </summary>
        public bool TeamCity {get; set;}

        /// <summary>
        /// Causes the user profile to be loaded in any separate test processes.
        /// </summary>
        public bool LoadUserProfile {get; set;}

        /// <summary>
        /// Lists all extension points and the extensions installed on each of them.
        /// </summary>
        public bool ListExtensions {get; set;}

        /// <summary>
        /// Gets or sets the principal policy for the test domain.
        /// </summary>
        public NUnit3PrincipalPolicy PrincipalPolicy {get; set;}

        /// <summary>
        /// Suppress display of program information at start of run.
        /// </summary>
        public bool NoHeader {get; set;}

        /// <summary>
        /// Gets or sets whether to display console output without color.
        /// </summary>
        public bool NoColor {get; set;}

        /// <summary>
        /// Allows additional undefined arguments to be provided.
        /// </summary>
        public List<string> AdditionalArguments {get; set;}


        public NUnit3Arguments()
        {
            InputFiles = new List<string>();
            ArgumentFiles = new List<string>();
            TestNames = new List<string>();
            Parameters = new Dictionary<string, string>(StringComparer.Ordinal);
            ProcessIsolation = NUnit3ProcessIsolation.Default;
            PrincipalPolicy = NUnit3PrincipalPolicy.Default;
            AdditionalArguments = new List<string>();
        }

        /// <summary>
        /// Returns the collection of argument strings defined by this instance.
        /// </summary>
        public IEnumerable<string> GetArguments()
        {
            foreach (var file in InputFiles ?? Enumerable.Empty<string>())
                yield return $"\"{file}\"";

            // TODO: @FILE

            if (TestNames?.Any() ?? false)
                yield return $"--test=\"{string.Join(",", TestNames)}\"";

            if (!string.IsNullOrEmpty(TestListFile))
                yield return $"--testlist=\"{TestListFile}\"";

            if (!string.IsNullOrEmpty(Where))
                yield return $"--where=\"{Where}\"";

            foreach (var key in Parameters?.Keys ?? Enumerable.Empty<string>())
                yield return $"--p=\"{key}={Parameters?[key]}\"";

            if (!string.IsNullOrEmpty(Configuration))
                yield return $"--config=\"{Configuration}\"";

            if (ProcessIsolation != NUnit3ProcessIsolation.Default)
                yield return $"--process={GetProcessIsolationString()}";

            if (InProcess)
                yield return "--inprocess";

            if (NumberOfAgents > 0)
                yield return $"--agents={NumberOfAgents}";

            if (DomainIsolation != NUnit3DomainIsolation.Default)
                yield return $"--domain={GetDomainIsolationString()}";

            if (!string.IsNullOrEmpty(Framework))
                yield return $"--framework=\"{Framework}\"";

            if (ForceX86)
                yield return "--x86";

            if (DisposeRunners)
                yield return "--dispose-runners";

            if (Timeout > 0)
                yield return $"--timeout={Timeout}";

            if (Seed > 0)
                yield return $"--seed={Seed}";

            if (Workers > 0)
                yield return $"--workers={Workers}";

            if (StopOnError)
                yield return "--stoponerror";

            if (SkipNonTestAssemblies)
                yield return "--skipnontestassemblies";

            if (Debug)
                yield return "--debug";

            if (!string.IsNullOrEmpty(WorkPath))
                yield return $"--work=\"{WorkPath}\"";

            if (!string.IsNullOrEmpty(OutputPath))
                yield return $"--output=\"{OutputPath}\"";

            foreach (var result in ResultSpecs ?? Enumerable.Empty<string>())
                yield return $"--result=\"{result}\"";

            if (Explore)
                yield return "--explore";

            foreach (var result in ExploreSpecs ?? Enumerable.Empty<string>())
                yield return $"--explore=\"{result}\"";

            if (NoResult)
                yield return "--noresult";

            if (TraceLevel != NUnit3TraceLevel.Default)
                yield return $"--trace={GetTraceLevelString()}";

            if (Labels != NUnit3Labels.Default)
                yield return $"--labels={GetLabelsString()}";

            if (!string.IsNullOrEmpty(TestNameFormat))
                yield return $"--test-name-format=\"{TestNameFormat}\"";

            if (!string.IsNullOrEmpty(Encoding))
                yield return $"--encoding=\"{Encoding}\"";

            if (ShadowCopy)
                yield return "--shadowcopy";

            if (TeamCity)
                yield return "--teamcity";

            if (LoadUserProfile)
                yield return "--loaduserprofile";

            if (ListExtensions)
                yield return "--list-extensions";

            if (PrincipalPolicy != NUnit3PrincipalPolicy.Default)
                yield return $"--set-principal-policy={GetPrincipalPolicyString()}";

            if (NoHeader)
                yield return "--noheader";

            if (NoColor)
                yield return "--nocolor";

            foreach (var arg in AdditionalArguments)
                yield return arg;
        }

        private string GetProcessIsolationString()
        {
            switch (ProcessIsolation) {
                case NUnit3ProcessIsolation.Single: return "single";
                case NUnit3ProcessIsolation.Separate: return "separate";
                case NUnit3ProcessIsolation.Multiple: return "multiple";
                default: return null;
            }
        }

        private string GetDomainIsolationString()
        {
            switch (DomainIsolation) {
                case NUnit3DomainIsolation.None: return "none";
                case NUnit3DomainIsolation.Single: return "single";
                case NUnit3DomainIsolation.Multiple: return "multiple";
                default: return null;
            }
        }

        private string GetTraceLevelString()
        {
            switch (TraceLevel) {
                case NUnit3TraceLevel.Off: return "off";
                case NUnit3TraceLevel.Verbose: return "verbose";
                case NUnit3TraceLevel.Info: return "info";
                case NUnit3TraceLevel.Warning: return "warning";
                case NUnit3TraceLevel.Error: return "error";
                default: return null;
            }
        }

        private string GetLabelsString()
        {
            switch (Labels) {
                case NUnit3Labels.Off: return "off";
                case NUnit3Labels.On: return "on";
                case NUnit3Labels.Before: return "before";
                case NUnit3Labels.After: return "after";
                case NUnit3Labels.All: return "all";
                default: return null;
            }
        }

        private string GetPrincipalPolicyString()
        {
            switch (PrincipalPolicy) {
                case NUnit3PrincipalPolicy.UnauthenticatedPrincipal: return "UnauthenticatedPrincipal";
                case NUnit3PrincipalPolicy.NoPrincipal: return "NoPrincipal";
                case NUnit3PrincipalPolicy.WindowsPrincipal: return "WindowsPrincipal";
                default: return null;
            }
        }
    }
}
