using System.Diagnostics;
using NuGet.Protocol.Plugins;
using System.Threading.Tasks;

namespace CredentialProvider.RequestHandlers
{
    internal class InitializeRequestHandler : RequestHandlerBase<InitializeRequest, InitializeResponse>
    {
        public InitializeRequestHandler(TraceSource logger)
            : base(logger)
        {
        }

        public override Task<InitializeResponse> HandleRequestAsync(InitializeRequest request)
        {
            return Task.FromResult(new InitializeResponse(MessageResponseCode.Success));
        }
    }
}