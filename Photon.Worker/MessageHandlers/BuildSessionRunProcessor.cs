using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework.Agent;
using Photon.Library.Communication.Messages.Session;
using Photon.Worker.Internal;
using Photon.Worker.Internal.Clients;
using Photon.Worker.Internal.Session;
using System;
using System.Threading.Tasks;

namespace Photon.Worker.MessageHandlers
{
    internal class BuildSessionRunProcessor : MessageProcessorBase<WorkerBuildSessionRunRequest>
    {
        //private static readonly ILog Log = LogManager.GetLogger();

        public override async Task<IResponseMessage> Process(WorkerBuildSessionRunRequest requestMessage)
        {
            var context = this.GetMessageContext();

            var packageClient = new PackageClient(Transceiver, requestMessage.AgentSessionId);

            var applicationClient = new ApplicationClient(Transceiver) {
                AgentSessionId = requestMessage.AgentSessionId,
                ProjectId = requestMessage.Project.Id,
                //CurrentDeploymentNumber = 0, // TODO: BuildTask should not have access
            };

            var session = new WorkerBuildSession {
                //...
                BuildContext = new AgentBuildContext {
                    Project = requestMessage.Project,
                    Agent = requestMessage.Agent,
                    AssemblyFilename = requestMessage.AssemblyFilename,
                    GitRefspec = requestMessage.GitRefspec,
                    TaskName = requestMessage.TaskName,
                    WorkDirectory = requestMessage.WorkDirectory,
                    ContentDirectory = requestMessage.ContentDirectory,
                    BinDirectory = requestMessage.BinDirectory,
                    BuildNumber = requestMessage.BuildNumber,
                    Packages = packageClient,
                    ServerVariables = requestMessage.ServerVariables,
                    AgentVariables = requestMessage.AgentVariables,
                    Applications = applicationClient,
                    CommitHash = requestMessage.CommitHash,
                    CommitAuthor = requestMessage.CommitAuthor,
                    CommitMessage = requestMessage.CommitMessage,
                    //Output = contextOutput,
                },
            };

            // TODO: This sucks
            context.Session = session;

            PrintToConsole(context.Session.Context);

            try {
                await context.Session.Initialize(session.Token);

                context.Begin();

                await context.Run();
            }
            catch {
                context.Abort(); // TODO ???
                throw;
            }

            return new ResponseMessageBase();
        }

        private static void PrintToConsole(IAgentContext context)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Beginning Session...");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"Server Session    : {context.ServerSessionId}");
            Console.WriteLine($"Agent Session     : {context.AgentSessionId}");
            //Console.WriteLine($"Client Session    : {context.SessionClientId}");
            Console.WriteLine($"Work Directory    : {context.WorkDirectory}");
            Console.WriteLine($"Bin Directory     : {context.BinDirectory}");
            Console.WriteLine($"Content Directory : {context.ContentDirectory}");
            Console.WriteLine($"Assembly Filename : {context.AssemblyFilename}");
            Console.WriteLine($"Project ID        : {context.Project?.Id}");
            Console.WriteLine($"Project Name      : {context.Project?.Name}");
            Console.WriteLine($"Agent ID          : {context.Agent?.Id}");
            Console.WriteLine($"Agent Name        : {context.Agent?.Name}");
            Console.WriteLine();
        }
    }
}
