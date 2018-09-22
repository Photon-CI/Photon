using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Worker.Internal;
using System;
using System.IO.Pipes;
using System.Reflection;
using System.Threading.Tasks;
using Console = System.Console;

namespace Photon.Worker
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            try {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Photon Worker {Configuration.Version}");

                await RunTask(args);
                return 0;
            }
            catch (OperationCanceledException error) {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write($"[CANCELLED] {error.UnfoldMessages()}");
                return 2;
            }
            catch (Exception error) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"[ERROR] {error.UnfoldMessages()}");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(error.StackTrace);
                return 1;
            }
            finally {
                Console.ResetColor();

                // TODO: For testing only!
                await Task.Delay(800);
            }
        }

        private static async Task RunTask(string[] args) {
            var context = new MessageContext();

            context.Arguments.Process(args);
            context.Arguments.Validate();

            var registry = new MessageProcessorRegistry();
            registry.Scan(Assembly.GetExecutingAssembly());

            Console.CancelKeyPress += (sender, e) => {
                context.Abort();
            };

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Connecting...");

            using (var pipeIn = new AnonymousPipeClientStream(PipeDirection.In, context.Arguments.PipeOutHandle))
            using (var pipeOut = new AnonymousPipeClientStream(PipeDirection.Out, context.Arguments.PipeInHandle))
            using (var transceiver = new MessageTransceiver(registry)) {
                transceiver.Context = context;

                try {
                    var pipeInOut = new CombinedStream(pipeIn, pipeOut);
                    transceiver.Start(pipeInOut);

                    await context.Run();
                }
                finally {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("Disconnecting...");
                }
            }
        }
    }
}
