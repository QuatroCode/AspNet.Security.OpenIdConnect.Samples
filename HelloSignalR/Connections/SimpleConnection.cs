using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;

namespace HelloSignalR.Connections
{
    public class SimpleConnection : PersistentConnection
    {
        protected override async Task OnConnected(HttpRequest request, string connectionId)
        {
            await base.OnConnected(request, connectionId);
            var identity = request.HttpContext.User.Identity;
            var authenticatedOrNot = (identity.IsAuthenticated ? "Authenticated" : "Unauthenticated");
            Logger.LogInformation($"{authenticatedOrNot} connection {connectionId} has just connected.");
            await Connection.Send(connectionId, $"Connection is {authenticatedOrNot}");
            if (identity.IsAuthenticated)
            {
                await Connection.Send(connectionId, $"Authenticated username: ${identity.Name}");
            }
        }
        protected override async Task OnReceived(HttpRequest request, string connectionId, string data)
        {
            var identity = request.HttpContext.User.Identity;
            var authenticatedOrNot = identity.IsAuthenticated ? "authenticated" : "unauthenticated";
            var clientName = identity.IsAuthenticated ? identity.Name : "client";
            await Connection.Send(connectionId, $"Received an {authenticatedOrNot} message from {clientName}: {data}");
        }
    }
}
