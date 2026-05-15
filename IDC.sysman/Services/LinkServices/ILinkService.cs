

namespace SysMan.Services.LinkService
{
    public interface ILinkService
    {

        string GenerateOneTimeLink(string objectName, string bucketName);
        LinkObject? GetFilePath(string token);
    }
}