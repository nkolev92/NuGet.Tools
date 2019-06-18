using Knapcode.MiniZip;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PackageExplorer
{
    internal class PackagePersistenceUtility
    {
        ILogger<PackagePersistenceUtility> _logger;
        public PackagePersistenceUtility(ILogger<PackagePersistenceUtility> logger)
        {
            _logger = logger;
        }
        private static MZipFormat _mZipFormat = new MZipFormat();

        public async Task<bool> StorePackageOnDiskAsync(HttpZipProvider httpZipProvider, Uri url, PackageInfo packageInfo)
        {
            try
            {
                var folder = Path.GetDirectoryName(packageInfo.LocalNupkgPath);

                if (!File.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                using (var zipDirectoryReader = await httpZipProvider.GetReaderAsync(url))
                using (FileStream destStream = new FileStream(packageInfo.LocalNupkgPath, FileMode.Create, FileAccess.Write))
                {
                    await _mZipFormat.WriteAsync(zipDirectoryReader.Stream, destStream);
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to save nupkg {url} to {packageInfo.LocalNupkgPath}");
                _logger.LogError(e.ToString());
            }
            return false;
        }

        public async Task<bool> StoreNuspecOnDiskAsync(HttpClient httpClient, Uri url, PackageInfo packageInfo)
        {
            try
            {
                var folder = Path.GetDirectoryName(packageInfo.LocalNuspecPath);

                if (!File.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                using (var srcStream = await httpClient.GetStreamAsync(url))
                using (FileStream destStream = new FileStream(packageInfo.LocalNuspecPath, FileMode.Create, FileAccess.Write))
                {
                    await _mZipFormat.WriteAsync(srcStream, destStream);
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to save nuspec {url} to {packageInfo.LocalNuspecPath}");
                _logger.LogError(e.ToString());
            }
            return false;
        }

        public static async Task<ZipDirectory> ReadPackageFromDisk(string packagePath)
        {
            using (var zipStream = await _mZipFormat.ReadAsync(new FileStream(packagePath, FileMode.Open, FileAccess.Read)))
            using (var reader = new ZipDirectoryReader(zipStream))
            {
                return await reader.ReadAsync();
            }
        }

        public static Task<NuspecReader> ReadNuspecFromDisk(string packagePath)
        {
            return Task.FromResult(new NuspecReader(packagePath));
        }
    }
}
