using NuGet.Protocol.Plugins;
using System.Threading;
using System.Threading.Tasks;

namespace CredentialProvider
{
    class PluginToClientRequestHandler
    {
        public IConnection Connection { get; set; }

        public async Task<bool> SendRequest(Message message, CancellationToken token)
        {
            if (Connection != null)
            {
                await Connection.SendAsync(message, token);
                return true;
            }
            return false;
        }
    }
}
