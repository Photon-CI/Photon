namespace Photon.Framework
{
    public interface IReferenceItem
    {
        string SessionId {get;}

        bool IsExpired();
    }
}
