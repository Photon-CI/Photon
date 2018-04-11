﻿using Photon.Communication.Messages;

namespace Photon.Framework.TcpMessages
{
    public class ApplicationPackagePullRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string PackageId {get; set;}
        public string PackageVersion {get; set;}
    }
}
