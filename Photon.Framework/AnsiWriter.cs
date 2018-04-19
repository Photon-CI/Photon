using System;
using System.Collections.Generic;
using System.IO;

namespace Photon.Framework
{
    /// <summary>
    /// Supports writing strings and ANSI color codes to a <see cref="TextWriter"/>.
    /// </summary>
    public class AnsiWriter
    {
        /// <summary>
        /// When enabled, ANSI color codes are written to the console.
        /// Otherwise the default color-buffers are used.
        /// </summary>
        public bool AnsiEnabled {get;}

        /// <summary>
        /// The underlying <see cref="TextWriter"/>.
        /// </summary>
        public TextWriter Writer {get;}


        /// <summary>
        /// Creates a new instance of <see cref="AnsiWriter"/>
        /// using the specified <paramref name="writer"/>.
        /// </summary>
        public AnsiWriter(TextWriter writer, bool ansiEnabled = true)
        {
            this.Writer = writer;
            this.AnsiEnabled = ansiEnabled;
        }

        //-----------------------------
        #region Default Methods

        /// <summary>
        /// Writes a character to the text string or stream.
        /// </summary>
        /// <param name="value">The character to write to the text stream.</param>
        public AnsiWriter Write(char value)
        {
            Writer.Write(value);
            return this;
        }

        /// <summary>
        /// Writes a string to the text string or stream.
        /// </summary>
        /// <param name="value">The string to write.</param>
        public AnsiWriter Write(string value)
        {
            Writer.Write(value);
            return this;
        }

        /// <summary>
        /// Writes the text representation of an object to the text string
        /// or stream by calling the ToString method on that object.
        /// </summary>
        /// <param name="value">The object to write.</param>
        public AnsiWriter Write(object value)
        {
            Writer.Write(value);
            return this;
        }

        /// <summary>
        /// Writes a line terminator to the text string or stream.
        /// </summary>
        public AnsiWriter WriteLine()
        {
            Writer.WriteLine();
            return this;
        }

        /// <summary>
        /// Writes a string followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The string to write.</param>
        public AnsiWriter WriteLine(string value)
        {
            Writer.WriteLine(value);
            return this;
        }

        /// <summary>
        /// Writes the text representation of an object by calling the ToString method
        /// on that object, followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The object to write.</param>
        public AnsiWriter WriteLine(object value)
        {
            Writer.WriteLine(value);
            return this;
        }

        #endregion
        //-----------------------------
        #region Color Methods

        /// <summary>
        /// Writes a character to the text string or stream using
        /// the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="value">The character to write to the text stream.</param>
        /// <param name="color">The color set before writing the string.</param>
        public AnsiWriter Write(char value, ConsoleColor color)
        {
            SetForegroundColor(color);
            Writer.Write(value);
            return this;
        }

        /// <summary>
        /// Writes a string to the text string or stream using
        /// the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <param name="color">The color set before writing the string.</param>
        public AnsiWriter Write(string value, ConsoleColor color)
        {
            SetForegroundColor(color);
            Writer.Write(value);
            return this;
        }

        /// <summary>
        /// Writes the text representation of an object to the text
        /// string or stream using the specified <paramref name="color"/>
        /// by calling the ToString method on that object.
        /// </summary>
        /// <param name="value">The object to write.</param>
        /// <param name="color">The color set before writing the string.</param>
        public AnsiWriter Write(object value, ConsoleColor color)
        {
            SetForegroundColor(color);
            Writer.Write(value);
            return this;
        }

        /// <summary>
        /// Writes a string followed by a line terminator to the text
        /// string or stream using the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <param name="color">The color set before writing the string.</param>
        public AnsiWriter WriteLine(string value, ConsoleColor color)
        {
            SetForegroundColor(color);
            Writer.WriteLine(value);
            return this;
        }

        /// <summary>
        /// Writes the text representation of an object by calling the ToString method
        /// on that object using the specified <paramref name="color"/>, followed by
        /// a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The object to write.</param>
        /// <param name="color">The color set before writing the string.</param>
        public AnsiWriter WriteLine(object value, ConsoleColor color)
        {
            SetForegroundColor(color);
            Writer.WriteLine(value);
            return this;
        }

        #endregion
        //-----------------------------

        /// <summary>
        /// Sets the foreground color of the console.
        /// </summary>
        public AnsiWriter SetForegroundColor(ConsoleColor color)
        {
            if (AnsiEnabled)
                Writer.Write(GetColorChars(color));
            else
                Console.ForegroundColor = color;

            return this;
        }

        /// <summary>
        /// Sets the foreground and background console colors to their defaults.
        /// </summary>
        public AnsiWriter ResetColor()
        {
            if (AnsiEnabled)
                Writer.Write("\x1b[0m");
            else
                Console.ResetColor();

            return this;
        }

        private static string GetColorChars(ConsoleColor color)
        {
            if (colorMap.TryGetValue(color, out var colorChars))
                return $"\x1b[{colorChars}m";

            throw new ApplicationException($"No color found matching '{color}'!");
        }

        private static readonly Dictionary<ConsoleColor, string> colorMap = new Dictionary<ConsoleColor, string>
        {
            [ConsoleColor.Gray] = "1;30",
            [ConsoleColor.Red] = "1;31",
            [ConsoleColor.Green] = "1;32",
            [ConsoleColor.Yellow] = "1;33",
            [ConsoleColor.Blue] = "1;34",
            [ConsoleColor.Magenta] = "1;35",
            [ConsoleColor.Cyan] = "1;36",
            [ConsoleColor.White] = "1;37",

            [ConsoleColor.Black] = "0;30",
            [ConsoleColor.DarkRed] = "0;31",
            [ConsoleColor.DarkGreen] = "0;32",
            [ConsoleColor.DarkYellow] = "0;33",
            [ConsoleColor.DarkBlue] = "0;34",
            [ConsoleColor.DarkMagenta] = "0;35",
            [ConsoleColor.DarkCyan] = "0;36",
            [ConsoleColor.DarkGray] = "0;37",
        };
    }
}
