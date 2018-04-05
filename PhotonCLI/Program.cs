﻿using Photon.CLI.Commands;
using Photon.CLI.Internal;
using System;
using AnsiConsole;

#if DEBUG
using System.Diagnostics;
#endif

namespace Photon.CLI
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            try {
                ConsoleEx.EnabledAnsi = false;

                var commandContext = new CommandContext();
                commandContext.Initialize();

                var rootCommands = new RootCommands(commandContext);

                rootCommands.ParseAsync(args)
                    .GetAwaiter().GetResult();

                return 0;
            }
            catch (ApplicationException error) {
                ConsoleEx.Out
                    .Write("An error has occurred! ", ConsoleColor.Red)
                    .WriteLine(error.Message, ConsoleColor.DarkRed);

                return 1;
            }
            catch (Exception error) {
                ConsoleEx.Out
                    .WriteLine("An unhandled exception has occurred!", ConsoleColor.Red)
                    .WriteLine(error.ToString(), ConsoleColor.DarkRed);

                return 2;
            }
            finally {
                ConsoleEx.Out.ResetColor();

#if DEBUG
                if (Debugger.IsAttached)
                    Console.ReadKey(true);
#endif
            }
        }
    }
}
