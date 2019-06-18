using Knapcode.MiniZip;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PackageExplorer
{
    class PackagePersistenceUtility
    {
        private static MZipFormat _mZipFormat = new MZipFormat();

        public static async Task<bool> StorePackageOnDiskAsync(HttpZipProvider httpZipProvider, Uri url, string packagePath)
        {
            try
            {
                var folder = Path.GetDirectoryName(packagePath);

                if (!File.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                using (var zipDirectoryReader = await httpZipProvider.GetReaderAsync(url))
                using (FileStream destStream = new FileStream(packagePath, FileMode.Create, FileAccess.Write))
                {
                    await _mZipFormat.WriteAsync(zipDirectoryReader.Stream, destStream);
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to save {url} to {packagePath}");
                Console.Write(e.ToString());
            }
            return false;
        }

        public static async Task<ZipDirectory> ReadPackageFromDisk(string packagePath)
        {
            using (var zipDirReader = new ZipDirectoryReader(new FileStream(packagePath, FileMode.Open, FileAccess.Read)))
            {
                return await zipDirReader.ReadAsync();
            }
        }
    }
}
