using System.Linq.Expressions;
using Microsoft.Extensions.Caching.Memory;

namespace SysMan.Services.LinkService
{
    public class LinkService : ILinkService
    {

        private readonly IMemoryCache _cache;
        private readonly TimeSpan _oneTimeLinkExpiration;

        public LinkService(IMemoryCache memoryCache, IConfiguration configuration)
        {
            _cache = memoryCache;
            var minutes = configuration.GetValue<int>("LinkService:OneTimeLinkExpirationMinutes", 10);
            _oneTimeLinkExpiration = TimeSpan.FromMinutes(minutes);
        }

        public string GenerateOneTimeLink(string objectName, string bucketName)
        {
            var token = Guid.NewGuid().ToString();
            var link = new OneTimeLink
            {
                Token = token,
                ObjectName = objectName,
                BucketName = bucketName,
                IsUsed = false
            };

            _cache.Set(token, link, _oneTimeLinkExpiration);
            return $"/preview-file?token={token}";
        }

        public LinkObject? GetFilePath(string token)
        {
            if (_cache.TryGetValue(token, out OneTimeLink link))
            {
                if (link.IsUsed)
                {
                    _cache.Remove(token);
                    return null;
                }

                link.IsUsed = true;
                _cache.Set(token, link, _oneTimeLinkExpiration);

                return new LinkObject
                {
                    ObjectName = link.ObjectName,
                    BucketName = link.BucketName,
                };
            }
            return null;
        }
    }
}