using NUnit.Framework;

namespace Photon.Tests.Internal
{
    internal class IntegrationTestAttribute : CategoryAttribute
    {
        public IntegrationTestAttribute() : base("integration") {}
    }
}
