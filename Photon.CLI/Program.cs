using Photon.CLI.Commands;
using Photon.CLI.Internal;
using Photon.Framework.Extensions;
using System;
using Photon.CLI.Internal.Http;

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
            catch (HttpStatusCodeException error) {
                ConsoleEx.Out.WriteLine(error.Message, ConsoleColor.DarkYellow);
                return 1;
            }
            catch (ApplicationException error) {
                ConsoleEx.Out.WriteLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);
                return 1;
            }
            catch (Exception error) {
                ConsoleEx.Out
                    .WriteLine("An unhandled exception has occurred!", ConsoleColor.Red)
                    .WriteLine(error.ToString(), ConsoleColor.DarkRed);

                return 2;
            }
            finally {
                Console.Out.Flush();
                ConsoleEx.Out.ResetColor();

#if DEBUG
                if (Debugger.IsAttached) {
                    ConsoleEx.Out.WriteLine("Press any key to exit...");
                    Console.ReadKey(true);
                }
#endif
            }
        }
    }
}
