using NuGet.Packaging.Core;
using NuGet.Versioning;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace PackageExplorer
{
    internal class PackageInfo
    {

        public string PackageId { get; }

        public string PackageVersion { get; }

        public string LocalNupkgPath { get; set; }

        public string LocalNuspecPath { get; set; }

        public Uri ContentUri { get; set; } // Comes from flat container

        public Uri NuspecUri { get; set; } // Comes from registration.

        private Lazy<PackageIdentity> packageIdentity;

        public PackageInfo(string packageId, string packageVersion, string path, Uri contentUri, Uri nuspecUri)
        {
            PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
            PackageVersion = packageVersion ?? throw new ArgumentNullException(nameof(packageVersion));
            ContentUri = contentUri ?? throw new ArgumentNullException(nameof(contentUri));
            NuspecUri = nuspecUri ?? throw new ArgumentNullException(nameof(nuspecUri));

            if (path == null) throw new ArgumentNullException(nameof(path));

            LocalNupkgPath = Path.Combine(path, $"{packageId.ToLower()}.mnupkg");
            LocalNuspecPath = Path.Combine(path, $"{packageId.ToLower()}.nuspec");

        }

        public PackageIdentity PackageIdentity
        {
            get
            {
                if (packageIdentity == null)
                {
                    packageIdentity = new Lazy<PackageIdentity>(() => new PackageIdentity(PackageId, NuGetVersion.Parse(PackageVersion)));
                }
                return packageIdentity?.Value;
            }
        }
    }
}
