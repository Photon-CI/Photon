using Photon.Framework.Agent;
using Photon.Framework.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Framework.Extensions
{
    public static class OutputExtensions
    {
        public static void WriteActionBlock(this IWriteBlocks writer, IDomainContext context, string action, IDictionary<string, object> parameters = null)
        {
            using (var block = writer.WriteBlock()) {
                if (context is IAgentContext agentContext)
                    block.Write($"[{agentContext.Agent.Name}] ", ConsoleColor.White);

                block.WriteLine(action, ConsoleColor.White);

                if (parameters?.Any() ?? false) {
                    foreach (var key in parameters.Keys) {
                        block.Write($"    {key} : ", ConsoleColor.DarkCyan);
                        block.WriteLine(parameters[key], ConsoleColor.Cyan);
                    }
                }
            }
        }

        public static void WriteErrorBlock(this IWriteBlocks writer, IDomainContext context, string message)
        {
            using (var block = writer.WriteBlock()) {
                if (context is IAgentContext agentContext)
                    block.Write($"[{agentContext.Agent.Name}] ", ConsoleColor.DarkRed);

                if (!string.IsNullOrEmpty(message))
                    block.WriteLine($"    {message}", ConsoleColor.DarkYellow);
            }
        }
    }
}
