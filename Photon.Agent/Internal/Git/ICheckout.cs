﻿using Photon.Agent.Internal.Session;

namespace Photon.Agent.Internal.Git
{
    internal interface ICheckout
    {
        void Checkout(string refspec = "master");
    }
}
