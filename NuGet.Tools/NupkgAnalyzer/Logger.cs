using System.Threading.Tasks;
using NuGet.Common;

namespace NupkgAnalyzer
{
    internal class Logger : ILogger
    {
        private LogLevel information;

        public Logger(LogLevel information)
        {
            this.information = information;
        }

        public void Log(LogLevel level, string data)
        {
        }

        public void Log(ILogMessage message)
        {
        }

        public Task LogAsync(LogLevel level, string data)
        {
            return null;
        }

        public Task LogAsync(ILogMessage message)
        {
            return null;
        }

        public void LogDebug(string data)
        {
        }

        public void LogError(string data)
        {
        }

        public void LogInformation(string data)
        {
        }

        public void LogInformationSummary(string data)
        {
        }

        public void LogMinimal(string data)
        {
        }

        public void LogVerbose(string data)
        {
        }

        public void LogWarning(string data)
        {
        }
    }
}