using NUnit.Framework;
using System;

namespace Photon.Tests.Internal
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    internal class IntegrationTestAttribute : CategoryAttribute
    {
        public IntegrationTestAttribute() : base("integration") {}
    }
}
