namespace Photon.Library
{
    public interface IReferenceItem
    {
        string SessionId {get;}

        bool IsExpired();
    }
}
