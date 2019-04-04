using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine(string.Join(";", args));

            if (args.Length != 2 && args.Length != 3)
            {
                Console.WriteLine("You need to provide the root and folder count");
                return -1;
            }

            var root = args[0];
            var folderCount = int.Parse(args[1]);

            if (args.Length == 3)
            {
                DirectoryUtility.OriginalBehavior = Boolean.Parse(args[2]);
            }

            Console.WriteLine($"Root is {root}, folder count is {folderCount}");
            var tasks = new List<Task<int>>();

            for (int i = 1; i <= folderCount; i++)
            {
                tasks.Add(RunTask(Path.Combine(root, "artifacts", $"{i.ToString()}00", "msbuild", "obj")));
            }
            try 
            {
                Task.WhenAll(tasks).Wait();
            } 
            catch(Exception e) 
            {
                while (DirectoryUtility.LogQueue.TryDequeue(out var logMessage))
                {
                    Console.WriteLine(logMessage);
                }

                throw e;
            }
            return 0;
        }

        private static async Task<int> RunTask(string folderName)
        {
            await Task.Delay(50);
            DirectoryUtility.CreateSharedDirectory(folderName);
            return 0;
        }
    }

    public static class DirectoryUtility
    {
        private static ConcurrentDictionary<string, object> ConcurrentDictionary = new ConcurrentDictionary<string, object>();
        internal static ConcurrentQueue<string> LogQueue = new ConcurrentQueue<string>();
        internal static bool OriginalBehavior = false;

        /// <summary>
        /// Creates all directories and subdirectories in the specified path unless they already exist.
        /// New directories can be read and written by all users.
        /// </summary>
        public static void CreateSharedDirectory(string path)
        {
            //LogQueue.Enqueue($"CreateSharedDirectory: {path}");

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
                //LogQueue.Enqueue($"CreateSharedDirectory: {path} In the loop currently at: {currentPath}");
                if (!Directory.Exists(currentPath))
                {
                    //LogQueue.Enqueue($"CreateSharedDirectory: {path} Running CreateSingleSharedDirectory {currentPath}");
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
            LogQueue.Enqueue($"CreateSingleSharedDirectory {path} parentDir: {parentDir} tempDir: {tempDir} CreateDirectory temp!");
            Directory.CreateDirectory(tempDir);
            LogQueue.Enqueue($"CreateSingleSharedDirectory {path} parentDir: {parentDir} tempDir: {tempDir} Created directory!");
            if (chmod(tempDir, UGO_RWX) == -1)
            {
                // it's very unlikely we can't set the permissions of a directory we just created
                TryDeleteDirectory(tempDir);
                var errno = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"{DateTime.Now.Ticks}: Unable to set permission while creating {path}, errno={errno}.");
            }
            try
            {
                LogQueue.Enqueue($"CreateSingleSharedDirectory {path} moving directory {tempDir}  to {path}");

                if (!OriginalBehavior)
                {
                    lock (ConcurrentDictionary.GetOrAdd(path, lockObject => new object()))
                    {
                        Directory.Move(tempDir, path);
                    }
                }
                else
                {
                    Directory.Move(tempDir, path);
                }
                LogQueue.Enqueue($"CreateSingleSharedDirectory {path} Done moving      {tempDir}  to {path}");

            }
            catch
            {
                //Console.WriteLine($"CreateSingleSharedDirectory {path} Failed moving    {tempDir}  to {path}");
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
