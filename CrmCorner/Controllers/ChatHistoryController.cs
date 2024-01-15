using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Models
{
    public class ChatHistoryController
    {
        private readonly CrmcornerContext _context;

        public ChatHistoryController(CrmcornerContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ChatHistory chatHistory)
        {
            await _context.InsertOneAsync(chatHistory);
        }

        //public async Task<IList<ChatHistory>> GetListAsync()
        //{
        //    var chatHistories = _context.ChatHistories.AsQueryable();

        //    return await chatHistories.ToListAsync();
        //}

        //public async Task<ChatHistory> GetByIdAsync(string id)
        //{
        //    return await _context.ChatHistories.Where(m=>m.Id==id).FirstOrDefaultAsync();
        //}
    }
}