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

                var program = new Program();

                // TODO: Load Configuration Data
                PhotonServer.Instance.Queue.MaxDegreeOfParallelism = 3;

                return program.Run(args);
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

        private int Run(string[] args)
        {
            var arguments = new Arguments();

            try {
                arguments.Parse(args);
            }
            catch (Exception error) {
                Log.Fatal("Failed to parse arguments!", error);
                return 1;
            }

            if (arguments.RunAsConsole) {
                var result = 0;
                try {
                    RunAsConsole(args);
                }
                catch (Exception error) {
                    Log.Fatal("Unhandled Exception!", error);
                    result = 1;
                }

                if (result != 0) {
                    try {
                        Console.ResetColor();
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey(true);
                    }
                    catch {}
                }

                return result;
            }
            else {
                ServiceBase.Run(new [] {
                    new ServerService(),
                });

                return 0;
            }
        }

        private void RunAsConsole(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Photon Server");
            Console.ResetColor();

            PhotonServer.Instance.Start();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Server Started");
            Console.ResetColor();
            Console.ReadKey(true);

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Server Stopping...");
            Console.ResetColor();

            PhotonServer.Instance.Stop();
            Console.WriteLine();
        }
    }
}
