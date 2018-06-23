using NUnit.Framework;
using Photon.Framework.Variables;
using Photon.Tests.Internal;
using System;

namespace Photon.Tests.Variables
{
    [TestFixture, UnitTest]
    public class VariableWrapperTests
    {
        private readonly VariableSet wrapper;


        public VariableWrapperTests()
        {
            var variable = new {
                String = "World World!",
                Bool = true,
                Integer = 1,
                DateTime = DateTime.MaxValue,
                Null = (object)null,
                Path1 = new {
                    Path2 = new {
                        Path3 = "PathValue",
                    }
                }
            };

            wrapper = new VariableSet(variable);
        }

        [Test]
        public void GetStringValueSuccessfully()
        {
            Assert.That(wrapper.GetValue("String"), Is.EqualTo("World World!"));
        }

        [Test]
        public void GetBoolValueSuccessfully()
        {
            Assert.That(wrapper.GetValue("Bool"), Is.EqualTo(true));
        }

        [Test]
        public void GetIntValueSuccessfully()
        {
            Assert.That(wrapper.GetValue("Integer"), Is.EqualTo(1));
        }

        [Test]
        public void GetNullValueSuccessfully()
        {
            Assert.That(wrapper.GetValue("Null"), Is.Null);
        }

        [Test]
        public void GetDeepPathValueSuccessfully()
        {
            Assert.That(wrapper.GetValue("Path1.Path2.Path3"), Is.EqualTo("PathValue"));
        }

        [Test]
        public void NullVariableThrowsException()
        {
            Assert.Throws(typeof(ArgumentNullException), () => {
                var _ = new VariableSet(null);
            });
        }

        [Test]
        public void NullInPathThrowsException()
        {
            Assert.Throws(typeof(ArgumentNullException), () => wrapper.GetValue("Null.NotFound"));
        }
    }
}
