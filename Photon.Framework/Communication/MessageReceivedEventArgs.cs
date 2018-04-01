using System;

namespace Photon.Framework.Communication
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
