namespace AI_Chatbot_Backend.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string ChatId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
