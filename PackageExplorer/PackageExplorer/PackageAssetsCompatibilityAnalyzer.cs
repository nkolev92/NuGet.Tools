using NuGet.Client;
using NuGet.ContentModel;
using NuGet.Frameworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageExplorer
{
    class PackageAssetsCompatibilityAnalyzer
    {

        public static IEnumerable<NuGetFramework> GetCompatibleFrameworks(IEnumerable<string> fileList, string folder)
        {

            if (fileList == null) return Enumerable.Empty<NuGetFramework>();
            if (folder == null) return Enumerable.Empty<NuGetFramework>();
            var managedCodeConventions = new ManagedCodeConventions(new NuGet.RuntimeModel.RuntimeGraph());

            var collection = new ContentItemCollection();
            collection.Load(fileList);

            // Act
            var groups = collection.FindItemGroups(managedCodeConventions.Patterns.RuntimeAssemblies)
                .OrderBy(group => ((NuGetFramework)group.Properties["tfm"]).GetShortFolderName())
                .ToList();
            return groups.Select(e => (NuGetFramework)e.Properties["tfm"]);
        }
    }
}
