using log4net;
using log4net.Config;
using Photon.Server.Internal;
using System;
using System.ServiceProcess;

namespace Photon.Server
{
    internal static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        public static PhotonServer Server {get; set;}


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
                LogManager.Flush(3000);
            }
        }

        private static int Run(string[] args) {
            try {
                Arguments.Parse(args);
            }
            catch (Exception error) {
                Log.Error("Failed to parse arguments!", error);
                return 1;
            }

            if (Arguments.RunAsConsole)
                return RunAsConsole(args);

            ServiceBase.Run(new [] {
                new ServerService(),
            });

            return 0;
        }

        private static int RunAsConsole(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Photon Server");
            Console.ResetColor();

            try {
                Server = new PhotonServer();
                Server.Start();

                Console.ReadKey(true);
            }
            finally {
                Server?.Dispose();
                //Server = null;
            }

            return 0;
        }
    }
}
