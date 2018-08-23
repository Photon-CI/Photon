namespace Photon.Library.Http.Security
{
    public interface IAuthorize
    {
        HttpUserContext Authorize(HttpUserCredentials credentials);
    }
}
