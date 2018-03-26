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

        public static ServerSessionManager Sessions {get;}
        public static ScriptQueue Queue {get;}
        //public static ServerTaskManager Tasks {get;}
        public static PhotonServer Server {get;}


        static Program()
        {
            Sessions = new ServerSessionManager();
            Queue = new ScriptQueue();
            //Tasks = new ServerTaskManager();
            Server = new PhotonServer();
        }

        public static int Main(string[] args)
        {
            try {
                XmlConfigurator.Configure();

                // TODO: Load from Configuration
                Queue.MaxDegreeOfParallelism = 3;
                //Tasks.Initialize();

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

        private static int Run(string[] args)
        {
            try {
                Arguments.Parse(args);
            }
            catch (Exception error) {
                Log.Fatal("Failed to parse arguments!", error);
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
                Server.Start();
                Sessions.Start();
                Queue.Start();

                Console.ReadKey(true);

                Queue.Stop();
                Sessions.Stop();
                Server.Stop();
            }
            finally {
                Server?.Dispose();
                Sessions?.Dispose();
            }

            return 0;
        }
    }
}
