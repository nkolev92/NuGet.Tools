using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PackageExplorer
{
    class PackageInfoFactory
    {
        private SourceRepository _sourceRepository;
        private Uri _packageBaseAddress;
        private VersionFolderPathResolver _versionFolderPathResolver;
        private int _initialized;

        public PackageInfoFactory(SourceRepository sourceRepository, VersionFolderPathResolver versionFolderPathResolver)
        {
            _sourceRepository = sourceRepository;
            _versionFolderPathResolver = versionFolderPathResolver;
        }

        public async Task<PackageInfo> CreatePackageInfo(string id, string version)
        {
            await EnsureInitializedAsync();
            return new PackageInfo(id, version, _versionFolderPathResolver.GetInstallPath(id, version), new Uri(GetFlatContainerNupkgUri(id, version)), new Uri(GetFlatContainerNuspecUri(id, version)));
        }

        public async Task EnsureInitializedAsync()
        {
            if (Interlocked.CompareExchange(ref _initialized, 1, 0) == 0)
            {
                var serviceIndexResource = await _sourceRepository.GetResourceAsync<ServiceIndexResourceV3>(CancellationToken.None);
                _packageBaseAddress = serviceIndexResource.GetServiceEntryUri(ServiceTypes.PackageBaseAddress);
            }
        }

        internal string GetFlatContainerNupkgUri(string id, string version)
        {
            return _packageBaseAddress + id.ToLowerInvariant() + "/" + version.ToLowerInvariant() + "/" + id.ToLowerInvariant() + "." + version.ToLowerInvariant() + ".nupkg";
        }

        internal string GetFlatContainerNuspecUri(string id, string version)
        {
            return _packageBaseAddress + id.ToLowerInvariant() + "/" + version.ToLowerInvariant() + "/" + id.ToLowerInvariant() + ".nuspec";
        }
    }
}
