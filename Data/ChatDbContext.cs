using Microsoft.EntityFrameworkCore;
using AI_Chatbot_Backend.Models;

namespace AI_Chatbot_Backend.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}
