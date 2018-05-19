using System;

namespace Photon.Library.Session
{
    public class SessionStatusArgs : EventArgs
    {
        public object Data {get; set;}

        public SessionStatusArgs(object data)
        {
            this.Data = data;
        }
    }
}
