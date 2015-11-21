using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;

namespace HelloSignalR.Connections
{
    public class SimpleConnection : PersistentConnection
    {
        protected override async Task OnConnected(HttpRequest request, string connectionId)
        {
            await base.OnConnected(request, connectionId);
            var identity = request.HttpContext.User.Identity;
            await Connection.Send(connectionId, $"Connection is {(identity.IsAuthenticated ? "authenticated" : "unauthenticated")}");
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
