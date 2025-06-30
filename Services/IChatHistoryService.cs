using AI_Chatbot_Backend.Models;

namespace AI_Chatbot_Backend.Services
{
    public interface IChatHistoryService
    {
        Task<List<ChatMessage>> GetHistoryAsync(string chatId);
        Task AddMessagesAsync(string chatId, List<MessageDTO> newMessages);
    }
}
