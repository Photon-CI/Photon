using System;

namespace Photon.Library
{
    public abstract class LifespanReferenceItem : IReferenceItem
    {
        private readonly DateTime utcStarted;

        public string Id {get;}
        public TimeSpan Lifespan {get; set;}


        public LifespanReferenceItem()
        {
            Id = Guid.NewGuid().ToString("N");
            utcStarted = DateTime.UtcNow;
        }

        public virtual bool IsExpired()
        {
            var elapsed = DateTime.UtcNow - utcStarted;
            return elapsed > Lifespan;
        }
    }
}
