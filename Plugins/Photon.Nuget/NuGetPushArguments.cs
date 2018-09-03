using System;
using System.Collections.Generic;

namespace Photon.NuGetPlugin
{
    /// <summary>
    /// Creates a NuGet package based on the specified .nuspec or project file.
    /// </summary>
    public class NuGetPushArguments
    {
        /// <summary>
        /// Identifies the package to push to the server.
        /// </summary>
        public string PackagePath {get; set;}

        /// <summary>
        /// The API key for the target repository. If not present,
        /// the one specified in the config file is used.
        /// </summary>
        public string ApiKey {get; set;}

        /// <summary>
        /// The NuGet configuration file to apply. If not specified,
        /// %AppData%\NuGet\NuGet.Config (Windows) or
        /// ~/.nuget/NuGet/NuGet.Config (Mac/Linux) is used.
        /// </summary>
        public string ConfigFile {get; set;}

        /// <summary>
        /// Disables buffering when pushing to an HTTP(s) server to decrease memory usages.
        /// Caution: when this option is used, integrated Windows authentication might not work.
        /// </summary>
        public bool DisableBuffering {get; set;}

        /// <summary>
        /// (3.5+) Forces nuget.exe to run using an invariant, English-based culture.
        /// </summary>
        public bool ForceEnglishOutput {get; set;}

        /// <summary>
        /// Suppresses prompts for user input or confirmations.
        /// </summary>
        public bool NonInteractive {get; set;}

        /// <summary>
        /// (3.5+) If a symbols package exists, it will not be pushed to a symbol server.
        /// </summary>
        public bool NoSymbols {get; set;}

        /// <summary>
        /// Specifies the server URL. NuGet identifies a UNC or local folder source and
        /// simply copies the file there instead of pushing it using HTTP. Also, starting
        /// with NuGet 3.4.2, this is a mandatory parameter unless the NuGet.Config file
        /// specifies a DefaultPushSource value (see Configuring NuGet behavior).
        /// </summary>
        public string Source {get; set;}

        /// <summary>
        /// (3.5+) Specifies the symbol server URL; nuget.smbsrc.net is used when pushing to nuget.org
        /// </summary>
        public string SymbolSource {get; set;}

        /// <summary>
        /// (3.5+) Specifies the API key for the URL specified in -SymbolSource.
        /// </summary>
        public string SymbolApiKey {get; set;}

        /// <summary>
        /// Specifies the timeout, in seconds, for pushing to a server.
        /// The default is 300 seconds (5 minutes).
        /// </summary>
        public int Timeout {get; set;} = 300;

        /// <summary>
        /// Specifies the amount of detail displayed in the output.
        /// </summary>
        public NuGetVerbosity Verbosity {get; set;} = NuGetVerbosity.Normal;

        /// <summary>
        /// Allows additional undefined arguments to be provided.
        /// </summary>
        public List<string> AdditionalArguments {get; set;}


        public NuGetPushArguments()
        {
            AdditionalArguments = new List<string>();
        }

        /// <summary>
        /// Returns the collection of argument strings defined by this instance.
        /// </summary>
        public IEnumerable<string> GetArguments()
        {
            if (string.IsNullOrEmpty(PackagePath)) throw new ArgumentNullException(nameof(PackagePath), "PackagePath must be specified!");

            yield return "push";

            yield return $"\"{PackagePath}\"";

            if (!string.IsNullOrEmpty(ApiKey))
                yield return $"-ApiKey \"{ApiKey}\"";

            if (!string.IsNullOrEmpty(ConfigFile))
                yield return $"-ConfigFile \"{ConfigFile}\"";

            if (DisableBuffering)
                yield return "-DisableBuffering";

            if (ForceEnglishOutput)
                yield return "-ForceEnglishOutput";

            if (NonInteractive)
                yield return "-NonInteractive";

            if (NoSymbols)
                yield return "-NoSymbols";

            if (!string.IsNullOrEmpty(Source))
                yield return $"-Source \"{Source}\"";

            if (!string.IsNullOrEmpty(SymbolSource))
                yield return $"-SymbolSource \"{SymbolSource}\"";

            if (!string.IsNullOrEmpty(SymbolApiKey))
                yield return $"-SymbolApiKey \"{SymbolApiKey}\"";

            if (Timeout > 0)
                yield return $"-Timeout {Timeout}";

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
    }
}
