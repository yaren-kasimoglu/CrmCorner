using CrmCorner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

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

            private readonly static ConnectionMapping<string> _connections = new ConnectionMapping<string>();

            public override async Task OnDisconnectedAsync(Exception? exception)
            {
                var claims = Context.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                if (claims != null)
                {
                    _connections.Add(claims.Value, Context.ConnectionId);
                }
                await base.OnDisconnectedAsync(exception);
            }

            public async Task SendMessage(string message, string receiverUserId, string dateTime)
            {
                var senderClaims = Context.User.Claims.FirstOrDefault(c =>
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                if (senderClaims == null) return;

                var senderUserId = senderClaims.Value;

                var receiverConnections = _connections.GetConnections(receiverUserId).ToList();
                bool isOnline = receiverConnections.Any();

                await AddChatHistory(message, senderUserId, receiverUserId, isOnline);

                // 🔹 Sadece alıcının bağlantılarına gönder (kendine değil!)
                foreach (var connectionId in receiverConnections)
                {
                    // 💬 Mesaj içeriği gönder
                    await Clients.Client(connectionId)
                        .SendAsync("ChatChannel", message, dateTime, senderUserId);

                    // 🔔 Bildirim (yalnızca alıcıya)
                    await Clients.Client(connectionId)
                        .SendAsync("ReceiveNotification", senderUserId);
                }
            }


            public async Task MarkMessagesAsRead(string senderId)
            {
                var receiverId = Context.User.Claims.FirstOrDefault(c =>
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

                if (receiverId == null) return;

                var unreadMessages = _context.ChatHistories
                    .Where(x => x.SenderId == senderId && x.ReceiverId == receiverId && !x.IsRead)
                    .ToList();

                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }

                await _context.SaveChangesAsync();

                // 💬 Karşı taraftaki kullanıcıya bildir
                var senderConnections = _connections.GetConnections(senderId);
                foreach (var conn in senderConnections)
                {
                    // 🔹 Burada receiverId değil senderId gönderiyoruz!
                    await Clients.Client(conn).SendAsync("MessagesMarkedAsRead", senderId);
                }
            }




            private async Task AddChatHistory(string message, string senderId, string receiverId, bool isRead)
            {
                var history = new ChatHistory
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Message = message,
                    IsRead = isRead,
                    MessageTime = DateTime.Now
                };

                try
                {
                    _context.ChatHistories.Add(history);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            public async Task SendFile(string user, string fileName, byte[] fileData)
            {
                await Clients.All.SendAsync("ReceiveFile", user, fileName, fileData);
            }
        }
    }
}
