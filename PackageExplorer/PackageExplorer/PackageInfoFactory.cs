using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PackageExplorer
{
    class PackageInfoFactory
    {
        private SourceRepository _sourceRepository;
        private Uri _packageBaseAddress;
        private RegistrationResourceV3 _registrationResourceV3;

        private int _initialized;


        public PackageInfoFactory(SourceRepository sourceRepository)
        {
            _sourceRepository = sourceRepository;
        }

        public async Task<PackageInfo> CreatePackageInfo(string id, string version)
        {
            await EnsureInitializedAsync();
            return new PackageInfo(id, version, "randomPath", new Uri(GetFlatContainerNupkgUri(id, version)), _registrationResourceV3.GetUri(id, version));
        }

        public async Task EnsureInitializedAsync()
        {
            if (Interlocked.CompareExchange(ref _initialized, 1, 0) == 0)
            {
                var serviceIndexResource = await _sourceRepository.GetResourceAsync<ServiceIndexResourceV3>(CancellationToken.None);
                _packageBaseAddress = serviceIndexResource.GetServiceEntryUri(ServiceTypes.PackageBaseAddress);
                _registrationResourceV3 = await _sourceRepository.GetResourceAsync<RegistrationResourceV3>(CancellationToken.None);
            }
        }

        internal string GetFlatContainerNupkgUri(string id, string version)
        {
            return _packageBaseAddress + id.ToLowerInvariant() + "/" + version + "/" + id.ToLowerInvariant() + "." + version + ".nupkg";
        }
    }
}
