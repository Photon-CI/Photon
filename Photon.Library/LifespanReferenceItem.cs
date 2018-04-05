using System;

namespace Photon.Library
{
    public abstract class LifespanReferenceItem : IReferenceItem
    {
        private readonly DateTime utcStarted;

        public string SessionId {get;}
        public TimeSpan Lifespan {get; set;}


        public LifespanReferenceItem()
        {
            SessionId = Guid.NewGuid().ToString("N");
            utcStarted = DateTime.UtcNow;
        }

        public virtual bool IsExpired()
        {
            var elapsed = DateTime.UtcNow - utcStarted;
            return elapsed > Lifespan;
        }
    }
}
