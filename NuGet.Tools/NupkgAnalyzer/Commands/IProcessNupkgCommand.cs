using System.Collections.Generic;
using System.IO.Compression;
using NuGet.Protocol;

namespace NupkgAnalyzer
{
    public interface IProcessNupkgCommand
    {
        Dictionary<string, string> Execute(ZipArchive archive, LocalPackageInfo localPackage);

    }
}
