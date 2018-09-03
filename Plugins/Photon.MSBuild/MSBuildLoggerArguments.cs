using System;
using System.Collections.Generic;

namespace Photon.MSBuild
{
    public class MSBuildLoggerArguments
    {
        /// <summary>
        /// Pass the parameters that you specify to the console logger,
        /// which displays build information in the console window.
        /// </summary>
        public MSBuildConsoleLoggerParameters ConsoleLoggerParameters {get; set;}

        /// <summary>
        /// Override the verbosity setting for this logger.
        /// </summary>
        public MSBuildVerbosityLevels? Verbosity {get; set;}

        /// <summary>
        /// Log the build output of each MSBuild node to its own file. The initial location
        /// for these files is the current directory. By default, the files are named
        /// "MSBuildNodeId.log". You can use the <see cref="FileLoggerParameters"/> switch to
        /// specify the location of the files and other parameters for the fileLogger.
        ///
        /// If you name a log file by using the <see cref="FileLoggerParameters"/> switch, the
        /// distributed logger will use that name as a template and append the node ID to that
        /// name when creating a log file for each node.
        /// </summary>
        public string DistributedFileLogger {get; set;}

        /// <summary>
        /// Log events from MSBuild, attaching a different logger instance to each node.
        /// </summary>
        public List<string> DistributedLoggers {get; set;}

        /// <summary>
        /// Log the build output to a single file in the current directory. If you don't specify
        /// number, the output file is named msbuild.log. If you specify number, the output file
        /// is named msbuildn.log, where n is number. Number can be a digit from 1 to 9.
        ///
        /// You can use the <see cref="FileLoggerParameters"/> switch to specify the location
        /// of the file and other parameters for the fileLogger.
        /// </summary>
        public FileLoggerIndex FileLogger {get; set;}

        /// <summary>
        /// Specifies any extra parameters for the file logger and the distributed file logger.
        /// The presence of this switch implies that the corresponding filelogger[number] switch
        /// is present. Number can be a digit from 1 to 9.
        ///
        /// You can use all parameters that are listed for <see cref="ConsoleLoggerParameters"/>.
        /// </summary>
        public FileLoggerParameterIndex FileLoggerParameters {get; set;}

        /// <summary>
        /// Specifies the logger to use to log events from MSBuild.
        /// </summary>
        public List<string> Loggers {get; set;}

        /// <summary>
        /// Disable the default console logger, and don't log events to the console.
        /// </summary>
        public bool NoConsoleLogger {get; set;}


        public MSBuildLoggerArguments()
        {
            ConsoleLoggerParameters = MSBuildConsoleLoggerParameters.Default;
            DistributedLoggers = new List<string>();
            FileLogger = new FileLoggerIndex();
            FileLoggerParameters = new FileLoggerParameterIndex();
            Loggers = new List<string>();
        }

        /// <summary>
        /// Returns the collection of argument strings defined by this instance.
        /// </summary>
        public IEnumerable<string> GetArguments()
        {
            if (ConsoleLoggerParameters != MSBuildConsoleLoggerParameters.Default) {
                var args = new List<string>();

                if (ConsoleLoggerParameters.HasFlag(MSBuildConsoleLoggerParameters.PerformanceSummary))
                    args.Add("PerformanceSummary");

                if (ConsoleLoggerParameters.HasFlag(MSBuildConsoleLoggerParameters.Summary))
                    args.Add("Summary");

                if (ConsoleLoggerParameters.HasFlag(MSBuildConsoleLoggerParameters.NoSummary))
                    args.Add("NoSummary");

                if (ConsoleLoggerParameters.HasFlag(MSBuildConsoleLoggerParameters.ErrorsOnly))
                    args.Add("ErrorsOnly");

                if (ConsoleLoggerParameters.HasFlag(MSBuildConsoleLoggerParameters.WarningsOnly))
                    args.Add("WarningsOnly");

                if (ConsoleLoggerParameters.HasFlag(MSBuildConsoleLoggerParameters.NoItemAndPropertyList))
                    args.Add("NoItemAndPropertyList");

                if (ConsoleLoggerParameters.HasFlag(MSBuildConsoleLoggerParameters.ShowCommandLine))
                    args.Add("ShowCommandLine");

                if (ConsoleLoggerParameters.HasFlag(MSBuildConsoleLoggerParameters.ShowTimestamp))
                    args.Add("ShowTimestamp");

                if (ConsoleLoggerParameters.HasFlag(MSBuildConsoleLoggerParameters.ShowEventId))
                    args.Add("ShowEventId");

                if (ConsoleLoggerParameters.HasFlag(MSBuildConsoleLoggerParameters.ForceNoAlign))
                    args.Add("ForceNoAlign");

                if (ConsoleLoggerParameters.HasFlag(MSBuildConsoleLoggerParameters.DisableConsoleColor))
                    args.Add("DisableConsoleColor");

                if (ConsoleLoggerParameters.HasFlag(MSBuildConsoleLoggerParameters.DisableMPLogging))
                    args.Add("DisableMPLogging");

                if (ConsoleLoggerParameters.HasFlag(MSBuildConsoleLoggerParameters.EnableMPLogging))
                    args.Add("EnableMPLogging");

                yield return $"/clp:{string.Join(";", args)}";
            }

            if (Verbosity.HasValue)
                yield return $"/verbosity:{MSBuildVerbosityLevel.GetString(Verbosity.Value)}";

            if (!string.IsNullOrEmpty(DistributedFileLogger))
                yield return $"/dfl:\"{DistributedFileLogger}\"";

            foreach (var logger in DistributedLoggers)
                yield return $"/dl:\"{logger}\"";

            if (FileLogger.Default) yield return "/fl";

            foreach (var key in FileLoggerParameters.Default.Keys)
                yield return $"/flp:{key}=\"{FileLoggerParameters.Default[key]}\"";

            for (var i = 1; i < 10; i++) {
                if (FileLogger[i]) yield return $"/fl{i}";

                foreach (var key in FileLoggerParameters[i].Keys)
                    yield return $"/flp{i}:{key}=\"{FileLoggerParameters[i][key]}\"";
            }

            foreach (var logger in Loggers)
                yield return $"/l:\"{logger}\"";

            if (NoConsoleLogger)
                yield return "/noconlog";
        }

        public class FileLoggerIndex
        {
            private readonly IDictionary<int, bool> _fileLoggers;


            public FileLoggerIndex()
            {
                _fileLoggers = new Dictionary<int, bool>();
            }

            public bool this[int index] {
                get => GetValue(index, true);
                set => SetValue(index, value, true);
            }

            public bool Default {
                get => GetValue(0, false);
                set => SetValue(0, value, false);
            }

            private bool GetValue(int index, bool limit)
            {
                if (limit) ApplyLimit(index);
                return _fileLoggers.TryGetValue(index, out var value) && value;
            }

            private void SetValue(int index, bool value, bool limit)
            {
                if (limit) ApplyLimit(index);
                _fileLoggers[index] = value;
            }

            private void ApplyLimit(int index)
            {
                if (index < 1 || index > 9) throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be between 1 and 9.");
            }
        }

        public class FileLoggerParameterIndex
        {
            private readonly IDictionary<int, IDictionary<string, string>> _fileLoggerParameters;


            public FileLoggerParameterIndex()
            {
                _fileLoggerParameters = new Dictionary<int, IDictionary<string, string>>();
            }

            public IDictionary<string, string> this[int index] {
                get => GetOrCreate(index, true);
                //set => SetValue(index, value, true);
            }

            public IDictionary<string, string> Default {
                get => GetOrCreate(0, false);
                //set => SetValue(0, value, false);
            }

            private IDictionary<string, string> GetOrCreate(int index, bool limit)
            {
                if (limit) ApplyLimit(index);

                if (!_fileLoggerParameters.TryGetValue(index, out var value)) {
                    value = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    _fileLoggerParameters[index] = value;
                }

                return value;
            }

            //private IDictionary<string, string> GetValue(int index, bool limit)
            //{
            //    if (limit) ApplyLimit(index);
            //    return _fileLoggers.TryGetValue(index, out var value) ? value : null;
            //}

            //private void SetValue(int index, string value, bool limit)
            //{
            //    if (limit) ApplyLimit(index);
            //    _fileLoggers[index] = value;
            //}

            private void ApplyLimit(int index)
            {
                if (index < 1 || index > 9) throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be between 1 and 9.");
            }
        }
    }
}
