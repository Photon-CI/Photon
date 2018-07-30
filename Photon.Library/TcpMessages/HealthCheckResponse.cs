using Photon.Communication.Messages;
using System.Collections.Generic;

namespace Photon.Library.TcpMessages
{
    public class HealthCheckResponse : ResponseMessageBase
    {
        public List<string> Warnings {get; set;}
        public List<string> Errors {get; set;}


        public HealthCheckResponse()
        {
            Warnings = new List<string>();
            Errors = new List<string>();
        }
    }
}
