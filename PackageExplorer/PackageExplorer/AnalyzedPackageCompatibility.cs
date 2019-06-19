using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageExplorer
{
    public class PackageCompatibilityInfo
    {
        public string Id { get; }
        public string Version { get; }
        public IEnumerable<string> AllFiles { get; set; }

        public IEnumerable<string> DependencyGroupFrameworks { get; set; }

        public IEnumerable<string> LibFrameworks { get; set; }
        public IEnumerable<string> LibFiles { get; set; }


        public IEnumerable<string> RefFrameworks { get; set; }
        public IEnumerable<string> RefFiles { get; set; }


        public IEnumerable<string> RuntimeFrameworks { get; set; }
        public IEnumerable<string> RuntimeFiles { get; set; }


        public IEnumerable<string> MSBuildMultiTargetingFrameworks { get; set; }
        public IEnumerable<string> MSBuildMultiTargetingFiles { get; set; }


        public IEnumerable<string> MSBuildFrameworks { get; set; }
        public IEnumerable<string> MSBuildFiles { get; set; }


        public IEnumerable<string> MSBuildTransitiveFrameworks { get; set; }
        public IEnumerable<string> MSBuildTransitiveFiles { get; set; }


        public IEnumerable<string> AnyTargetFrameworks { get; set; }
        public IEnumerable<string> AnyTargetFiles { get; set; }


        public PackageCompatibilityInfo(string id, string version)
        {
            Id = id;
            Version = version;
        }

    }
}
