using NUnit.Framework;
using Photon.Library.TcpMessages.Status;
using Photon.Library.Worker;
using Photon.Tests.Internal;

namespace Photon.Tests.Workers
{
    [TestFixture, IntegrationTest]
    [Parallelizable(ParallelScope.All)]
    public class WorkerCollectionTests
    {
        private const string workerFilename = @"Z:\Photon\Photon.Worker\bin\Debug\Photon.Worker.exe";


        [Test]
        public void CreateWorker()
        {
            using (var workerCollection = new WorkerCollection()) {
                workerCollection.WorkerFilename = workerFilename;

                using (var worker = workerCollection.Create()) {
                    var request = new WorkerStatusGetRequest();

                    var messageTask = worker.Transceiver.Send(request)
                        .GetResponseAsync<WorkerStatusGetResponse>();

                    if (!messageTask.Wait(4_000)) Assert.Fail("Timeout waiting for message response!");

                    var response = messageTask.Result;

                    Assert.That(response.HostName, Is.EqualTo("?"));

                    worker.Disconnect();
                }
            }
        }
    }
}
