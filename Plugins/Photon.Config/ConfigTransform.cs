using Microsoft.Web.XmlTransform;
using Photon.Framework;
using Photon.Framework.Domain;
using Photon.Framework.Extensions;
using System;
using System.Collections.Generic;

namespace Photon.Config
{
    public class ConfigTransform
    {
        private readonly IDomainContext context;

        public IWriteBlocks Output {get; set;}


        public ConfigTransform() {}

        public ConfigTransform(IDomainContext context)
        {
            this.context = context;

            Output = context.Output;
        }

        public void Transform(string configFilename, string transformFilename, string outputFilename)
        {
            Output?.WriteActionBlock(context, "Applying config transformation",
                new Dictionary<string, object> {
                    ["Source"] = configFilename,
                    ["Transform"] = transformFilename,
                    ["Output"] = outputFilename,
                });

            try {
                using (var configDoc = new XmlTransformableDocument()) {
                    configDoc.PreserveWhitespace = true;
                    configDoc.Load(configFilename);

                    using (var transformDoc = new XmlTransformation(transformFilename)) {
                        if (!transformDoc.Apply(configDoc))
                            throw new ApplicationException("Failed to apply config transformation!");
                    }

                    configDoc.Save(outputFilename);
                }

                //Output?.WriteLine($"Applying configuration transform '{transformFilename}' to config file '{configFilename}'...", ConsoleColor.Gray);
            }
            catch (Exception error) {
                Output?.WriteErrorBlock(context, error.UnfoldMessages());

                throw;
            }
        }

        //private void WriteAction<T>(IWrite<T> writer, string action, IDictionary<string, object> parameters, ConsoleColor color = ConsoleColor.White)
        //    where T : IWrite<T>
        //{
        //    if (context is IAgentContext agentContext)
        //        writer.Write($"[{agentContext.Agent.Name}] ", color);

        //    writer.WriteLine(action, color);

        //    foreach (var key in parameters.Keys) {
        //        writer.Write($"    {key} : ", ConsoleColor.DarkCyan);
        //        writer.WriteLine(parameters[key], ConsoleColor.Cyan);
        //    }
        //}
    }
}
