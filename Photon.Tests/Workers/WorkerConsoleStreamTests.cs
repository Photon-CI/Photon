using NUnit.Framework;
using Photon.Library.TcpMessages.Session;
using Photon.Library.Worker;
using Photon.Tests.Internal;

namespace Photon.Tests.Workers
{
    [TestFixture, IntegrationTest]
    [Parallelizable(ParallelScope.All)]
    public class WorkerConsoleStreamTests
    {
        private const string workerFilename = @"Z:\Photon\Photon.Worker\bin\Debug\Photon.Worker.exe";


        [Test]
        public void SafeDisconnect()
        {
            using (var worker = new Worker()) {
                worker.Filename = workerFilename;
                worker.ShowConsole = true;

                worker.Start();
                worker.Transceiver.Context = null;

                var beginMessage = new WorkerTestSessionBeginRequest();

                var responseTask = worker.Transceiver.Send(beginMessage).GetResponseAsync();

                if (!responseTask.Wait(4_000)) Assert.Fail("Timeout waiting for message response!");

                worker.Disconnect();
            }
        }
    }
}
