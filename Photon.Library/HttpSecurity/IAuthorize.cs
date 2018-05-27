namespace Photon.Library.HttpSecurity
{
    public interface IAuthorize
    {
        HttpUserContext Authorize(HttpUserCredentials credentials);
    }
}
