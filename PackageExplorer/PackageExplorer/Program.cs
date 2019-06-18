using Knapcode.MiniZip;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PackageExplorer
{
    partial class Program
    {

        public static async Task<int> Main()
        {

            PackageInfoFactory packageInfoFactory = new PackageInfoFactory(Repository.Factory.GetCoreV3(NuGetConstants.V3FeedUrl));

            var packageId = "Newtonsoft.Json";
            var packageVersion = "10.0.3";


            var packageInfo = await packageInfoFactory.CreatePackageInfo(packageId, packageVersion);

            using (var httpClient = new HttpClient())
            {
                VersionFolderPathResolver versionFolderPathResolver = new VersionFolderPathResolver(@"F:\MZipDownloads");
                var httpZipProvider = new HttpZipProvider(httpClient);
                var packagePath = Path.Combine(versionFolderPathResolver.GetInstallPath(packageId, packageVersion), "package.mzip");
                if (await PackagePersistenceUtility.StorePackageOnDiskAsync(httpZipProvider, packageInfo.ContentUri, packagePath))
                {
                    await PackagePersistenceUtility.ReadPackageFromDisk(packagePath);

                }
                else
                {
                    return 1;
                }




                //Func<IEnumerable<string>, IEnumerable<string>> packageFileFilter = (list) => list.Where(e => ShouldInclude(e));
                //PrintList(await GetPackageFileListFromFlatContainerAsync(httpZipProvider, packageInfo.ContentUri, packageFileFilter));
            }
            return 0;
        }

        private static void PrintList(IEnumerable<string> list)
        {
            foreach (var entry in list)
            {
                Console.WriteLine(entry);
            }
        }

        // Not all the files from a zip file are needed
        // So, files such as '.rels' and '[Content_Types].xml' are not relevant
        private static bool ShouldInclude(string fullName)
        {
            var fileName = Path.GetFileName(fullName);
            if (fileName != null)
            {
                if (fileName == ".rels")
                {
                    return false;
                }
                if (fileName == "[Content_Types].xml")
                {
                    return false;
                }
            }

            var extension = Path.GetExtension(fullName);
            if (extension == ".psmdcp")
            {
                return false;
            }
            return true;
        }

        public static async Task<IEnumerable<string>> GetPackageFileListFromFlatContainerAsync(HttpZipProvider httpZipProvider, Uri url, Func<IEnumerable<string>, IEnumerable<string>> filter = null)
        {

            using (var zipDirectoryReader = await httpZipProvider.GetReaderAsync(url))
            {
                var zipDirectory = await zipDirectoryReader.ReadAsync();
                var entries = zipDirectory.Entries.Select(e => e.GetName());
                if (filter != null)
                {
                    entries = filter.Invoke(entries);
                }

                return entries;
            }
        }
    }
}
