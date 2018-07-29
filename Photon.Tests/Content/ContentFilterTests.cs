using NUnit.Framework;
using Photon.Framework.Tools.Content;
using System.Collections.Generic;

namespace Photon.Tests.Content
{
    [TestFixture]
    internal class ContentFilterTests
    {
        [Test]
        public void FindsEverything()
        {
            var directoriesFound = new List<string>();
            var filesFound = new List<string>();

            var filter = new ContentFilter {
                SourceDirectory = "C:\\Test\\Source",
                DestinationDirectory = "C:\\Test\\Destination",
                DirectoryAction = (src, dest) => directoriesFound.Add(src),
                FileAction = (src, dest) => filesFound.Add(src),
                Provider = CreateMockProvider(),
            };

            filter.Run();

            Assert.That(directoriesFound, Contains.Item("C:\\Test\\Source\\A"));
            Assert.That(directoriesFound, Contains.Item("C:\\Test\\Source\\ignore"));
            Assert.That(filesFound, Contains.Item("C:\\Test\\Source\\rootFile.txt"));
            Assert.That(filesFound, Contains.Item("C:\\Test\\Source\\A\\B.zip"));
            Assert.That(filesFound, Contains.Item("C:\\Test\\Source\\ignore\\donotfind"));
        }

        [Test]
        public void IgnoresDirectories()
        {
            var directoriesFound = new List<string>();
            var filesFound = new List<string>();

            var filter = new ContentFilter {
                SourceDirectory = "C:\\Test\\Source",
                DestinationDirectory = "C:\\Test\\Destination",
                IgnoredDirectories = {
                    "ignore",
                },
                DirectoryAction = (src, dest) => directoriesFound.Add(src),
                FileAction = (src, dest) => filesFound.Add(src),
                Provider = CreateMockProvider(),
            };

            filter.Run();

            Assert.That(directoriesFound, Contains.Item("C:\\Test\\Source\\A"));
            Assert.That(directoriesFound, Does.Not.Contain("C:\\Test\\Source\\ignore"));
            Assert.That(filesFound, Contains.Item("C:\\Test\\Source\\rootFile.txt"));
            Assert.That(filesFound, Contains.Item("C:\\Test\\Source\\A\\B.zip"));
            Assert.That(filesFound, Does.Not.Contain("C:\\Test\\Source\\ignore\\donotfind"));
        }

        private IContentProvider CreateMockProvider()
        {
            return new MockContentProvider {
                Directories = {
                    "C:\\Test\\Source",
                    "C:\\Test\\Source\\A",
                    "C:\\Test\\Source\\ignore",
                    "C:\\Test\\Destination",
                },
                Files = {
                    "C:\\Test\\Source\\rootFile.txt",
                    "C:\\Test\\Source\\A\\B.zip",
                    "C:\\Test\\Source\\ignore\\donotfind",
                },
            };
        }
    }
}
