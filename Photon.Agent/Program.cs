using log4net;
using log4net.Config;
using Photon.Agent.Internal;
using System;
using System.ServiceProcess;

namespace Photon.Agent
{
    internal static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        public static PhotonAgent Agent {get;}
        public static AgentSessionManager Sessions {get;}


        static Program()
        {
            Agent = new PhotonAgent();
            Sessions = new AgentSessionManager();
        }

        public static int Main(string[] args)
        {
            try {
                XmlConfigurator.Configure();

                Sessions.Start();

                var result = Run(args);

                Sessions.Stop();

                return result;
            }
            catch (Exception error) {
                Log.Fatal("Unhandled Exception!", error);
                return -1;
            }
            finally {
                Sessions.Dispose();
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
                new AgentService(),
            });

            return 0;
        }

        private static int RunAsConsole(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Photon Agent");
            Console.ResetColor();

            try {
                Agent.Start();

                Console.ReadKey(true);

                Agent.Stop();
            }
            finally {
                Agent?.Dispose();
            }

            return 0;
        }
    }
}
