﻿using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TestApp
{
    public static class DirectoryUtility
    {

        /// <summary>
        /// Creates all directories and subdirectories in the specified path unless they already exist.
        /// New directories can be read and written by all users.
        /// </summary>
        public static void CreateSharedDirectory(string path)
        {
            path = Path.GetFullPath(path);
            if (Directory.Exists(path))
            {
                return;
            }
            // ensure directories exists starting from the root
            var root = Path.GetPathRoot(path);
            var sepPos = root.Length - 1;
            do
            {
                sepPos = path.IndexOf(Path.DirectorySeparatorChar, sepPos + 1);
                var currentPath = sepPos == -1 ? path : path.Substring(0, sepPos);

                if (!Directory.Exists(currentPath))
                {
                    CreateSingleSharedDirectory(currentPath);

                }
            } while (sepPos != -1);

        }

        private static void CreateSingleSharedDirectory(string path)
        {
            // Creating a directory and setting the permissions are two operations. To avoid race
            // conditions, we create a different directory, set the permissions and rename it. We
            // create it under the parent directory to make sure it is on the same volume.
            var parentDir = Path.GetDirectoryName(path);
            var tempDir = Path.Combine(parentDir, Guid.NewGuid().ToString());
            Program.LogQueue.Enqueue($"CreateSingleSharedDirectory {path} parentDir: {parentDir} tempDir: {tempDir} CreateDirectory temp!");
            Directory.CreateDirectory(tempDir);
            Program.LogQueue.Enqueue($"CreateSingleSharedDirectory {path} parentDir: {parentDir} tempDir: {tempDir} Created directory!");
            if (chmod(tempDir, UGO_RWX) == -1)
            {
                // it's very unlikely we can't set the permissions of a directory we just created
                TryDeleteDirectory(tempDir);
                var errno = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"{DateTime.Now.Ticks}: Unable to set permission while creating {path}, errno={errno}.");
            }
            try
            {
                Program.LogQueue.Enqueue($"CreateSingleSharedDirectory {path} moving directory {tempDir}  to {path}");
                Directory.Move(tempDir, path);
                Program.LogQueue.Enqueue($"CreateSingleSharedDirectory {path} Done moving      {tempDir}  to {path}");

            }
            catch
            {
                Program.LogQueue.Enqueue($"CreateSingleSharedDirectory {path} Failed moving    {tempDir}  to {path}");
                TryDeleteDirectory(tempDir);
                if (Directory.Exists(path))
                {
                    return;
                }
                else
                {
                    throw;
                }
            }
        }

        private static void TryDeleteDirectory(string path)
        {
            try
            {
                Directory.Delete(path);
            }
            catch
            { }
        }

        private const int UGO_RWX = 0x1ff; // 0777

        [DllImport("libc", SetLastError = true)]
        private static extern int chmod(string pathname, int mode);
    }
}
