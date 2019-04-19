using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TestApp
{
    public class Program
    {
        internal static ConcurrentQueue<string> LogQueue = new ConcurrentQueue<string>();

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
            var OriginalBehavior = false;
            if (args.Length == 3)
            {
                OriginalBehavior = bool.Parse(args[2]);
            }

            Console.WriteLine($"Root is {root}, folder count is {folderCount}, OriginalBehavior={OriginalBehavior}");
            var tasks = new List<Task<int>>();

            for (int i = 1; i <= folderCount; i++)
            {
                var path = Path.Combine(root, "artifacts", $"{i.ToString()}00", "msbuild", "obj");
                if (OriginalBehavior)
                {
                    tasks.Add(RunTask(path));
                }
                else
                {
                    tasks.Add(RunTask2(path));
                }
            }
            try
            {
                Task.WhenAll(tasks).Wait();
            }
            catch (Exception e)
            {
                while (LogQueue.TryDequeue(out var logMessage))
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

        private static async Task<int> RunTask2(string folderName)
        {
            await Task.Delay(50);
            DirectoryUtility2.CreateSharedDirectory(folderName);
            return 0;
        }
    }
}
