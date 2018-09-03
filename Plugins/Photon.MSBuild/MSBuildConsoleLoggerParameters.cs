using System;

namespace Photon.MSBuild
{
    [Flags]
    public enum MSBuildConsoleLoggerParameters
    {
        Default,

        /// <summary>
        /// Show the time that's spent in tasks, targets, and projects.
        /// </summary>
        PerformanceSummary,

        /// <summary>
        /// Show the error and warning summary at the end.
        /// </summary>
        Summary,

        /// <summary>
        /// Don't show the error and warning summary at the end.
        /// </summary>
        NoSummary,

        /// <summary>
        /// Show only errors.
        /// </summary>
        ErrorsOnly,

        /// <summary>
        /// Show only warnings.
        /// </summary>
        WarningsOnly,

        /// <summary>
        /// Don't show the list of items and properties that would appear at the
        /// start of each project build if the verbosity level is set to diagnostic.
        /// </summary>
        NoItemAndPropertyList,

        /// <summary>
        /// Show TaskCommandLineEvent messages.
        /// </summary>
        ShowCommandLine,

        /// <summary>
        /// Show the timestamp as a prefix to any message.
        /// </summary>
        ShowTimestamp,

        /// <summary>
        /// Show the event ID for each started event,
        /// finished event, and message.
        /// </summary>
        ShowEventId,

        /// <summary>
        /// Don't align the text to the size of the console buffer.
        /// </summary>
        ForceNoAlign,

        /// <summary>
        /// Use the default console colors for all logging messages.
        /// </summary>
        DisableConsoleColor,

        /// <summary>
        /// Disable the multiprocessor logging style of output when
        /// running in non-multiprocessor mode.
        /// </summary>
        DisableMPLogging,

        /// <summary>
        /// Enable the multiprocessor logging style even when running in
        /// non-multiprocessor mode. This logging style is on by default.
        /// </summary>
        EnableMPLogging,
    }
}
