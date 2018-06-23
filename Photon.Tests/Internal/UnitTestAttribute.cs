using NUnit.Framework;

namespace Photon.Tests.Internal
{
    internal class UnitTestAttribute : CategoryAttribute
    {
        public UnitTestAttribute() : base("unit") {}
    }
}
