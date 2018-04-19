using Photon.Framework;
using System;

namespace Photon.CLI.Internal
{
    /// <summary>
    /// Limited extension of <see cref="Console"/> adding support
    /// for colorizing text using ANSI color characters.
    /// </summary>
    public static class ConsoleEx
    {
        /// <summary>
        /// When enabled, ANSI color codes are written to the console.
        /// Otherwise the default color-buffers are used.
        /// </summary>
        public static bool EnabledAnsi {get; set;} = true;

        /// <summary>
        /// Gets the standard output stream wrapped in an <see cref="AnsiWriter"/>.
        /// </summary>
        public static AnsiWriter Out => new AnsiWriter(Console.Out, EnabledAnsi);

        /// <summary>
        /// Gets the standard error stream wrapped in an <see cref="AnsiWriter"/>.
        /// </summary>
        public static AnsiWriter Error => new AnsiWriter(Console.Error, EnabledAnsi);
    }
}
