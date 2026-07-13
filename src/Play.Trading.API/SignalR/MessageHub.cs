using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Play.Trading.API.StateMachines;

namespace Play.Trading.API.SignalR;

[Authorize]
public class MessageHub : Hub
{
    public async Task SendStatusAsync(PurchaseState status)
    {
        if (Clients == null) return;
        
        await Clients.User(Context.UserIdentifier!)
            .SendAsync("ReceivePurchaseStatus", status);
    }
}