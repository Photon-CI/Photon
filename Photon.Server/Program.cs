using log4net;
using log4net.Config;
using Photon.Server.Internal;
using System;
using System.ServiceProcess;

namespace Photon.Server
{
    internal class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));


        public static int Main(string[] args)
        {
            try {
                XmlConfigurator.Configure();

                return Run(args);
            }
            catch (Exception error) {
                Log.Fatal("Unhandled Exception!", error);
                return -1;
            }
            finally {
                PhotonServer.Instance?.Dispose();
                LogManager.Flush(3000);
            }
        }

        private static int Run(string[] args)
        {
            var arguments = new Arguments();

            try {
                arguments.Parse(args);
            }
            catch (Exception error) {
                Log.Fatal("Failed to parse arguments!", error);
                return 1;
            }

            if (arguments.RunAsConsole)
                return RunAsConsole();

            ServiceBase.Run(new ServiceBase[] {
                new ServerService(),
            });

            return 0;
        }

        private static int RunAsConsole()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Photon Server");
            Console.ResetColor();

            try {
                PhotonServer.Instance.Start();

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("Server Started");
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
                Console.WriteLine("Server Stopping...");
                Console.ResetColor();

                PhotonServer.Instance.Stop();
                Console.WriteLine();
            }
        }
    }
}
