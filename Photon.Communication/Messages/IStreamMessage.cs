using System;
using System.IO;

namespace Photon.Communication.Messages
{
    public interface IStreamMessage : IMessage
    {
        Func<Stream> StreamFunc {get; set;}
    }
}
