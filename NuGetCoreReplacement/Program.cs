using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetCoreReplacement
{
    public class Program
    {

        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            var sourceId = "sourceId";
            var cancellationToken = CancellationToken.None;
            var packageId = "packageId";
            var version = "version";
            var downloadPath = "downloadPath";

            var source = new PackageSource(sourceId);
            source.Credentials = new PackageSourceCredential(sourceId, "username", "password", isPasswordClearText: true, "basic");
            var sourceRepository = Repository.CreateSource(Repository.Provider.GetCoreV3(), source);
            var feed = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
            var downloader = await feed.GetPackageDownloaderAsync(new PackageIdentity(packageId, NuGetVersion.Parse(version)), new SourceCacheContext(), NullLogger.Instance, cancellationToken);
            await downloader.CopyNupkgFileToAsync(downloadPath, cancellationToken);
        }
    }
}
