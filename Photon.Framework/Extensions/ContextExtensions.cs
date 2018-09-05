using Photon.Framework.Agent;
using Photon.Framework.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Framework.Extensions
{
    public static class ContextExtensions
    {
        public static void WriteActionBlock(this IDomainContext context, string action, IDictionary<string, object> parameters = null)
        {
            using (var block = context.Output.WriteBlock()) {
                PrintAgentTag(context, block, ConsoleColor.White);

                block.WriteLine(action, ConsoleColor.White);

                if (parameters?.Any() ?? false) {
                    foreach (var key in parameters.Keys) {
                        block.Write($"    {key} : ", ConsoleColor.DarkCyan);
                        block.WriteLine(parameters[key], ConsoleColor.Cyan);
                    }
                }
            }
        }

        public static void WriteErrorBlock(this IDomainContext context, string message, string longMessage = null)
        {
            using (var block = context.Output.WriteBlock()) {
                PrintAgentTag(context, block, ConsoleColor.DarkRed);

                block.WriteLine(message, ConsoleColor.DarkRed);

                if (!string.IsNullOrEmpty(longMessage))
                    block.WriteLine(longMessage, ConsoleColor.DarkYellow);
            }
        }

        private static void PrintAgentTag(IDomainContext context, IWrite block, ConsoleColor color)
        {
            if (context is IAgentDeployContext agentContext)
                block.Write($"[{agentContext.Agent.Name}] ", color);
        }
    }
}
