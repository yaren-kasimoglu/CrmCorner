using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Models
{
    public class ChatHistoryController
    {
        private readonly CrmCornerContext _context;

        public ChatHistoryController(CrmCornerContext context)
        {
            _context = context;
        }

        //public async Task AddAsync(ChatHistory chatHistory)
        //{
        //    await _context.InsertOneAsync(chatHistory);
        //} //GERİ AL 23.011.2024

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