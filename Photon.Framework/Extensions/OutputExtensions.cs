using Photon.Framework.Agent;
using Photon.Framework.Domain;
using System;
using System.Collections.Generic;

namespace Photon.Framework.Extensions
{
    public static class OutputExtensions
    {
        public static void WriteActionBlock<T, Z>(this IWriteBlocks<T, Z> writer, IDomainContext context, string action, IDictionary<string, object> parameters)
            where T : IWrite<T>
            where Z : IBlockWriter<Z>
        {
            using (var block = writer.WriteBlock()) {
                if (context is IAgentContext agentContext)
                    block.Write($"[{agentContext.Agent.Name}] ", ConsoleColor.White);

                block.WriteLine(action, ConsoleColor.White);

                foreach (var key in parameters.Keys) {
                    block.Write($"    {key} : ", ConsoleColor.DarkCyan);
                    block.WriteLine(parameters[key], ConsoleColor.Cyan);
                }
            }
        }

        public static void WriteErrorBlock<T, Z>(this IWriteBlocks<T, Z> writer, IDomainContext context, string message)
            where T : IWrite<T>
            where Z : IBlockWriter<Z>
        {
            using (var block = writer.WriteBlock()) {
                if (context is IAgentContext agentContext)
                    block.Write($"[{agentContext.Agent.Name}] ", ConsoleColor.DarkRed);

                block.WriteLine($"    {message}", ConsoleColor.DarkYellow);
            }
        }
    }
}
