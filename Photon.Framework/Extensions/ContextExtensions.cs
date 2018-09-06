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
                var tag = GetAgentTag(context);
                var text = !string.IsNullOrEmpty(tag)
                    ? $"[{tag}] {action}" : action;

                block.WriteLine(text, ConsoleColor.White);

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
                var tag = GetAgentTag(context);
                var text = !string.IsNullOrEmpty(tag)
                    ? $"[{tag}] {message}" : message;

                block.WriteLine(text, ConsoleColor.DarkRed);

                if (!string.IsNullOrEmpty(longMessage))
                    block.WriteLine(longMessage, ConsoleColor.DarkYellow);
            }
        }

        public static void WriteTagLine(this IDomainContext context, string message, ConsoleColor color)
        {
            var tag = GetAgentTag(context);
            var text = !string.IsNullOrEmpty(tag)
                ? $"[{tag}] {message}" : message;

            context.Output.WriteLine(text, color);
        }

        public static string GetAgentTag(IDomainContext context)
        {
            if (context is IAgentDeployContext agentContext)
                return agentContext.Agent.Name;

            return null;
        }
    }
}
