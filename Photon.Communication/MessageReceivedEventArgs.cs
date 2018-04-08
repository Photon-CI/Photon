using System;
using Photon.Communication.Messages;

namespace Photon.Communication
{
    internal class MessageReceivedEventArgs : EventArgs
    {
        public IMessage Message {get;}

        public MessageReceivedEventArgs(IMessage message)
        {
            this.Message = message;
        }

        public T GetMessage<T>() where T : IMessage
        {
            return (T)Message;
        }
    }
}
