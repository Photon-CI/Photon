namespace Photon.Library
{
    public interface IReferenceItem
    {
        string Id {get;}

        bool IsExpired();
    }
}
