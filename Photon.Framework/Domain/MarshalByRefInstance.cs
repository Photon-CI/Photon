using System;
using System.Collections.Generic;
using System.Runtime.Remoting;

namespace Photon.Framework.Domain
{
    public abstract class MarshalByRefInstance : MarshalByRefObject, IDisposable
    {
        private bool _disposed;

        protected virtual IEnumerable<MarshalByRefObject> NestedMarshalByRefObjects {
            get {yield break;}
        }

 
        ~MarshalByRefInstance()
        {
            Dispose(false);
        }
 
        public sealed override object InitializeLifetimeService()
        {
            return null;
        }
 
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }
 
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
 
            Disconnect();
            _disposed = true;
        }
 
        private void Disconnect()
        {
            RemotingServices.Disconnect(this);
 
            foreach (var tmp in NestedMarshalByRefObjects)
                RemotingServices.Disconnect(tmp);
        }
    }
}
