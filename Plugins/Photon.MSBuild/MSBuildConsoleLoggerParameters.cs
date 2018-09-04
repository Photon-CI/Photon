using System;

namespace Photon.MSBuild
{
    [Flags]
    public enum MSBuildConsoleLoggerParameters
    {
        Default = 0,

        /// <summary>
        /// Show the time that's spent in tasks, targets, and projects.
        /// </summary>
        PerformanceSummary = 1,

        /// <summary>
        /// Show the error and warning summary at the end.
        /// </summary>
        Summary = 2,

        /// <summary>
        /// Don't show the error and warning summary at the end.
        /// </summary>
        NoSummary = 4,

        /// <summary>
        /// Show only errors.
        /// </summary>
        ErrorsOnly = 8,

        /// <summary>
        /// Show only warnings.
        /// </summary>
        WarningsOnly = 16,

        /// <summary>
        /// Don't show the list of items and properties that would appear at the
        /// start of each project build if the verbosity level is set to diagnostic.
        /// </summary>
        NoItemAndPropertyList = 32,

        /// <summary>
        /// Show TaskCommandLineEvent messages.
        /// </summary>
        ShowCommandLine = 64,

        /// <summary>
        /// Show the timestamp as a prefix to any message.
        /// </summary>
        ShowTimestamp = 128,

        /// <summary>
        /// Show the event ID for each started event,
        /// finished event, and message.
        /// </summary>
        ShowEventId = 256,

        /// <summary>
        /// Don't align the text to the size of the console buffer.
        /// </summary>
        ForceNoAlign = 512,

        /// <summary>
        /// Use the default console colors for all logging messages.
        /// </summary>
        DisableConsoleColor = 1024,

        /// <summary>
        /// Disable the multiprocessor logging style of output when
        /// running in non-multiprocessor mode.
        /// </summary>
        DisableMPLogging = 2048,

        /// <summary>
        /// Enable the multiprocessor logging style even when running in
        /// non-multiprocessor mode. This logging style is on by default.
        /// </summary>
        EnableMPLogging = 4096,
    }
}
