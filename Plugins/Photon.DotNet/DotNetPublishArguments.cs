using System;
using System.Collections.Generic;

namespace Photon.DotNetPlugin
{
    /// <summary>
    /// Packs the application and its dependencies into a folder for deployment to a hosting system.
    /// </summary>
    public class DotNetPublishArguments
    {
        /// <summary>
        /// The project to publish. If not specified,
        /// it defaults to the current directory.
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
        /// Specifies one or several target manifests to use to trim the set of packages
        /// published with the app. The manifest file is part of the output of the dotnet
        /// store command. This option is available starting with .NET Core 2.0 SDK.
        /// </summary>
        public List<string> ManifestFiles {get; set;}

        /// <summary>
        /// Doesn't build the project before publishing.
        /// It also implicitly sets the NoRestore flag.
        /// </summary>
        public bool NoBuild {get; set;}

        /// <summary>
        /// Ignores project-to-project (P2P) references and only builds the specified root project.
        /// </summary>
        public bool NoDependencies {get; set;}

        /// <summary>
        /// Doesn't execute an implicit restore during build.
        /// </summary>
        public bool NoRestore {get; set;}

        /// <summary>
        /// Specifies the path for the output directory. If not specified, it defaults to
        /// ./bin/[configuration]/[framework]/publish/ for a framework-dependent deployment
        /// or ./bin/[configuration]/[framework]/[runtime]/publish/ for a self-contained
        /// deployment. If the path is relative, the output directory generated is relative
        /// to the project file location, not to the current working directory.
        /// </summary>
        public string Output {get; set;}

        /// <summary>
        /// Publishes the .NET Core runtime with your application so the runtime doesn't
        /// need to be installed on the target machine. If a runtime identifier is specified,
        /// its default value is true.
        /// </summary>
        public bool SelfContained {get; set;}

        /// <summary>
        /// Specifies the target runtime. For a list of
        /// Runtime Identifiers (RIDs), see the RID catalog.
        /// </summary>
        public string Runtime {get; set;}

        /// <summary>
        /// Sets the verbosity level of the command.
        /// </summary>
        public DotNetVerbosityLevel Verbosity {get; set;}

        /// <summary>
        /// Defines the version suffix to replace the asterisk (*)
        /// in the version field of the project file.
        /// </summary>
        public string VersionSuffix {get; set;}

        /// <summary>
        /// Allows additional undefined arguments to be provided.
        /// </summary>
        public List<string> AdditionalArguments {get; set;}


        public DotNetPublishArguments()
        {
            AdditionalArguments = new List<string>();
            Verbosity = DotNetVerbosityLevel.Normal;
        }

        /// <summary>
        /// Returns the collection of argument strings defined by this instance.
        /// </summary>
        public IEnumerable<string> GetArguments()
        {
            yield return "publish";

            if (!string.IsNullOrEmpty(ProjectFile))
                yield return $"\"{ProjectFile}\"";

            if (!string.IsNullOrEmpty(Configuration))
                yield return $"--configuration \"{Configuration}\"";

            if (!string.IsNullOrEmpty(Framework))
                yield return $"--framework \"{Framework}\"";

            if (Force) yield return "--force";

            if (ManifestFiles != null) {
                foreach (var manifest in ManifestFiles)
                    yield return $"--manifest \"{manifest}\"";
            }

            if (NoBuild) yield return "--no-build";

            if (NoDependencies) yield return "--no-dependencies";

            if (NoRestore) yield return "--no-restore";

            if (!string.IsNullOrEmpty(Output))
                yield return $"--output \"{Output}\"";

            if (SelfContained) yield return "--self-contained";

            if (!string.IsNullOrEmpty(Runtime))
                yield return $"--runtime  \"{Runtime}\"";

            if (Verbosity != DotNetVerbosityLevel.Normal)
                yield return $"--verbosity {DotNetVerbosity.GetString(Verbosity)}";

            if (!string.IsNullOrEmpty(VersionSuffix))
                yield return $"--version-suffix \"{VersionSuffix}\"";

            foreach (var arg in AdditionalArguments)
                yield return arg;
        }
    }
}
