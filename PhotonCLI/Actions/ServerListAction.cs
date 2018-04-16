using AnsiConsole;
using Photon.CLI.Internal;
using System;
using System.Threading.Tasks;

namespace Photon.CLI.Actions
{
    internal class ServerListAction
    {
        public Task Run(CommandContext context)
        {
            var i = 0;
            foreach (var definition in context.Servers.Definitions) {
                if (i > 0) {
                    ConsoleEx.Out.WriteLine();
                }
                i++;

                ConsoleEx.Out
                    .Write("Name    : ", ConsoleColor.Gray).WriteLine(definition.Name, ConsoleColor.White)
                    .Write("URL     : ", ConsoleColor.Gray).WriteLine(definition.Url, ConsoleColor.White)
                    .Write("Primary : ", ConsoleColor.Gray).WriteLine(definition.Primary, ConsoleColor.White);
            }

            return Task.CompletedTask;
        }
    }
}
