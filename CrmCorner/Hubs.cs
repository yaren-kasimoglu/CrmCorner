using CrmCorner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Graph;
using System.Threading.Tasks;

namespace CrmCorner.Hubs
{
   public class Hubs
    {
       public class ChatHub : Hub

        {
            private readonly UserManager<AppUser> _userManager;
            private readonly CrmCornerContext _context;
            public ChatHub(CrmCornerContext context, UserManager<AppUser> userManager)
            {
                _context = context;
                _userManager = userManager;
            }

            private readonly static ConnectionMapping<string> _connections =
           new ConnectionMapping<string>();
            public override async Task OnConnectedAsync()
            {
                var claims = Context.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                _connections.Add(claims.Value, Context.ConnectionId);
            	
                    await base.OnConnectedAsync();
            }
            public async Task SendMessage(string message, string receiverUserId,string dateTime)
            {
                if (_connections.GetConnections(receiverUserId).Count() <= 0)
                {
                    AddChatHistory(message, receiverUserId,false);

                }
                else
                {
                    AddChatHistory(message, receiverUserId,true);

                }
                var targetUserConnectionId = _connections.GetConnections(receiverUserId).First();
                await Clients.Client(targetUserConnectionId).SendAsync("ChatChannel", message, dateTime);

            }
            private async Task AddChatHistory(string message, string receiverUserId,bool state)
            {
                var claims = Context.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                var sernderUserId = claims.Value;
                ChatHistory history = new ChatHistory();
                history.SenderId = sernderUserId;
                history.ReceiverId = receiverUserId;
                history.Message = message;
                history.IsRead = state;
                history.MessageTime = DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second);
                try
                {
                    _context.ChatHistories.Add(history);
                    _context.SaveChanges();

                }
                catch(Exception EX)
                {
                    Console.Write(EX.Message);
                }
            }

        }
    }
}
