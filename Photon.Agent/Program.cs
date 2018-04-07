using log4net;
using log4net.Config;
using Photon.Agent.Internal;
using System;
using System.ServiceProcess;
using System.Threading.Tasks;
using Photon.Agent.Commands;

namespace Photon.Agent
{
    internal class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));


        public static int Main(string[] args)
        {
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

            ServiceBase.Run(new ServiceBase[] {
                new AgentService(),
            });

            return 0;
        }

        private static int RunAsConsole()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Photon Agent");
            Console.ResetColor();

            try {
                PhotonAgent.Instance.Start();

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("Agent Started");
                Console.ResetColor();
                Console.ReadKey(true);
                return 0;
            }
            catch (Exception error) {
                Log.Fatal("Unhandled Exception!", error);

                Console.ResetColor();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);

                return 1;
            }
            finally {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Agent Stopping...");
                Console.ResetColor();

                PhotonAgent.Instance.Stop();
                Console.WriteLine();
            }
        }
    }
}
