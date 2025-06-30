using AI_Chatbot_Backend.Data;
using AI_Chatbot_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace AI_Chatbot_Backend.Services
{
    public class ChatHistoryService : IChatHistoryService
    {
        private readonly ChatDbContext _db;

        public ChatHistoryService(ChatDbContext db)
        {
            _db = db;
        }

        public async Task<List<ChatMessage>> GetHistoryAsync(string chatId)
        {
            return await _db.ChatMessages
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task AddMessagesAsync(string chatId, List<MessageDTO> newMessages)
        {
            var existingMessages = await GetHistoryAsync(chatId);

            foreach (var msg in newMessages)
            {
                if (!existingMessages.Any(m => m.Message == msg.Message && m.Role == msg.Role))
                {
                    _db.ChatMessages.Add(new ChatMessage
                    {
                        ChatId = chatId,
                        Role = msg.Role,
                        Message = msg.Message
                    });
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}
