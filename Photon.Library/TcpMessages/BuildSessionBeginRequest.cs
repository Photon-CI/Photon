﻿using Photon.Communication.Messages;
using Photon.Framework.Projects;

namespace Photon.Library.TcpMessages
{
    public class BuildSessionBeginRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string ServerSessionId {get; set;}
        public Project Project {get; set;}
        public string AssemblyFile {get; set;}
        public string TaskName {get; set;}
        public string GitRefspec {get; set;}
        public int BuildNumber {get; set;}
    }
}