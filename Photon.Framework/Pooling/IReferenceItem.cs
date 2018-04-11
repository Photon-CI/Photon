namespace Photon.Framework.Pooling
{
    public interface IReferenceItem
    {
        string SessionId {get;}

        bool IsExpired();
    }
}
