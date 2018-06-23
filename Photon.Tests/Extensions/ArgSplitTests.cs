using NUnit.Framework;
using Photon.Framework;
using Photon.Tests.Internal;

namespace Photon.Tests.Extensions
{
    [TestFixture, UnitTest]
    public class ArgSplitTests
    {
        [Test]
        public void IsNull()
        {
            ProcessRunner.SplitCommand(null, out var exe, out var args);
            Assert.That(exe, Is.Null);
            Assert.That(args, Is.EqualTo(string.Empty));
        }

        [Test]
        public void IsEmpty()
        {
            ProcessRunner.SplitCommand(null, out var exe, out var args);
            Assert.That(exe, Is.Null);
            Assert.That(args, Is.EqualTo(string.Empty));
        }

        [Test]
        public void SplitNoQuotes()
        {
            ProcessRunner.SplitCommand("program arg1 arg2", out var exe, out var args);
            Assert.That(exe, Is.EqualTo("program"));
            Assert.That(args, Is.EqualTo("arg1 arg2"));
        }

        [Test]
        public void SplitWithQuotes()
        {
            ProcessRunner.SplitCommand("\"program\" arg1 arg2", out var exe, out var args);
            Assert.That(exe, Is.EqualTo("program"));
            Assert.That(args, Is.EqualTo("arg1 arg2"));
        }

        [Test]
        public void SplitNoQuotes_NoArgs()
        {
            ProcessRunner.SplitCommand("program", out var exe, out var args);
            Assert.That(exe, Is.EqualTo("program"));
            Assert.That(args, Is.EqualTo(string.Empty));
        }

        [Test]
        public void SplitWithQuotes_NoArgs()
        {
            ProcessRunner.SplitCommand("\"program\"", out var exe, out var args);
            Assert.That(exe, Is.EqualTo("program"));
            Assert.That(args, Is.EqualTo(string.Empty));
        }

        [Test]
        public void TrimExeQuotes()
        {
            ProcessRunner.SplitCommand("\" program \" arg1 arg2", out var exe, out var args);
            Assert.That(exe, Is.EqualTo("program"));
            Assert.That(args, Is.EqualTo("arg1 arg2"));
        }

        [Test]
        public void TrimArgs()
        {
            ProcessRunner.SplitCommand("program   arg1   arg2", out var exe, out var args);
            Assert.That(exe, Is.EqualTo("program"));
            Assert.That(args, Is.EqualTo("arg1   arg2"));
        }
    }
}
