using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CrmCorner.Hubs
{
   public class Hubs
    {
       public class ChatHub : Hub
        {
           public async Task SendMessage(string user, string message,string date)
            {
              await Clients.All.SendAsync("ReceiveMessage", user, message,date);
            }

        }
    }
}
