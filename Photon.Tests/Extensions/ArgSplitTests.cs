using NUnit.Framework;
using Photon.Framework.Process;
using Photon.Tests.Internal;
using System;

namespace Photon.Tests.Extensions
{
    [TestFixture, UnitTest]
    public class ArgSplitTests
    {
        [Test]
        public void IsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => ProcessRunInfo.FromCommand(null));
        }

        [Test]
        public void IsEmpty_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => ProcessRunInfo.FromCommand(string.Empty));
        }

        [Test]
        public void SplitNoQuotes_WhitespaceArgs()
        {
            var info = ProcessRunInfo.FromCommand("program ");
            Assert.That(info.Filename, Is.EqualTo("program"));
            Assert.That(info.Arguments, Is.Null);
        }

        [Test]
        public void SplitNoQuotes()
        {
            var info = ProcessRunInfo.FromCommand("program arg1 arg2");
            Assert.That(info.Filename, Is.EqualTo("program"));
            Assert.That(info.Arguments, Is.EqualTo("arg1 arg2"));
        }

        [Test]
        public void SplitWithQuotes()
        {
            var info = ProcessRunInfo.FromCommand("\"program\" arg1 arg2");
            Assert.That(info.Filename, Is.EqualTo("program"));
            Assert.That(info.Arguments, Is.EqualTo("arg1 arg2"));
        }

        [Test]
        public void SplitNoQuotes_NoArgs()
        {
            var info = ProcessRunInfo.FromCommand("program");
            Assert.That(info.Filename, Is.EqualTo("program"));
            Assert.That(info.Arguments, Is.Null);
        }

        [Test]
        public void SplitWithQuotes_NoArgs()
        {
            var info = ProcessRunInfo.FromCommand("\"program\"");
            Assert.That(info.Filename, Is.EqualTo("program"));
            Assert.That(info.Arguments, Is.Null);
        }

        [Test]
        public void TrimExeQuotes()
        {
            var info = ProcessRunInfo.FromCommand("\" program \" arg1 arg2");
            Assert.That(info.Filename, Is.EqualTo("program"));
            Assert.That(info.Arguments, Is.EqualTo("arg1 arg2"));
        }

        [Test]
        public void TrimArgs()
        {
            var info = ProcessRunInfo.FromCommand("program   arg1   arg2");
            Assert.That(info.Filename, Is.EqualTo("program"));
            Assert.That(info.Arguments, Is.EqualTo("arg1   arg2"));
        }
    }
}
