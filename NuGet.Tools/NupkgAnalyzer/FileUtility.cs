using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NupkgAnalyzer
{
    class FileUtility
    {
        public void CreateFile(string path, bool deleteIfExists)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (File.Exists(path) && deleteIfExists)
            {
                File.Delete(path);
            }
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
        }

        private void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
