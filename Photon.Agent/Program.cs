using log4net;
using log4net.Config;
using Photon.Agent.Commands;
using Photon.Agent.Internal;
using Photon.Framework.Extensions;
using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Agent
{
    internal class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        private static CancellationTokenSource tokenSource;
        private static AgentService service;


        public static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += Domain_OnUnhandledException;

            tokenSource = new CancellationTokenSource();

            try {
                XmlConfigurator.Configure();

                return Run(args).GetAwaiter().GetResult();
            }
            catch (Exception error) {
                Log.Fatal("Unhandled Exception!", error);
                return -1;
            }
            finally {
                PhotonAgent.Instance?.Dispose();
                LogManager.Flush(3000);
            }
        }

        public static void Shutdown()
        {
            tokenSource.Cancel();
            service?.Stop();
        }

        private static async Task<int> Run(string[] args)
        {
            var arguments = new RootCommands();

            try {
                await arguments.ParseAsync(args);
            }
            catch (Exception error) {
                Log.Fatal("Failed to parse arguments!", error);
                return 1;
            }

            if (arguments.Debug)
                return RunAsConsole();

            service = new AgentService();

            ServiceBase.Run(new ServiceBase[] {service});

            return 0;
        }

        private static int RunAsConsole()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Photon Agent");
            Console.ResetColor();

            Console.CancelKeyPress += (o, e) => {
                tokenSource.Cancel();
                e.Cancel = true;
            };

            try {
                PhotonAgent.Instance.Start();

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("Agent Started");
                Console.ResetColor();
                Console.WriteLine("Press [x] to exit...");

                ConsoleKey? key;
                do {
                    key = null;

                    if (Console.KeyAvailable)
                        key = Console.ReadKey(true).Key;

                    try {
                        Task.Delay(200, tokenSource.Token)
                            .GetAwaiter().GetResult();
                    }
                    catch (TaskCanceledException) {}
                } while (!tokenSource.IsCancellationRequested && (!key.HasValue || key.Value != ConsoleKey.X));

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Agent Stopping...");
                Console.ResetColor();

                PhotonAgent.Instance.Stop();
                Console.WriteLine();
                return 0;
            }
            catch (ApplicationException error) {
                Log.Fatal("Application Exception!", error);

                Console.WriteLine();
                Console.ResetColor();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);

                return 1;
            }
            catch (Exception error) {
                Log.Fatal("Unhandled Exception!", error);
                LogManager.Flush(200);

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(error.UnfoldMessages());

                Console.WriteLine();
                Console.ResetColor();
                Console.ReadKey(true);

                return 2;
            }
        }

        private static void Domain_OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Fatal("An unhandled exception occurred!", e.ExceptionObject as Exception);
        }
    }
}
