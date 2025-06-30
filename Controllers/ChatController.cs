using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AI_Chatbot_Backend.Models;
using AI_Chatbot_Backend.Services;

namespace AI_Chatbot_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatController> _logger;
        private readonly IChatHistoryService _chatHistoryService;

        public ChatController(IConfiguration configuration, ILogger<ChatController> logger, IChatHistoryService chatHistoryService)
        {
            _configuration = configuration;
            _logger = logger;
            _chatHistoryService = chatHistoryService;
        }

        [HttpPost("complete")]
        public async Task<IActionResult> Complete([FromQuery] string? chatId, [FromBody] List<MessageDTO> messages)
        {
            try
            {
                // Generate a new chatId if missing
                if (string.IsNullOrWhiteSpace(chatId))
                {
                    chatId = Guid.NewGuid().ToString();
                }

                // Store new user messages
                await _chatHistoryService.AddMessagesAsync(chatId, messages);

                // Retrieve full chat history
                var history = await _chatHistoryService.GetHistoryAsync(chatId);

                var apiKey = _configuration["OpenRouter:ApiKey"];
                var model = _configuration["OpenRouter:Model"] ?? "qwen/qwen3-32b:free";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:3000");
                client.DefaultRequestHeaders.Add("X-Title", "AI Chatbot");

                // Prepare OpenRouter-compatible message format
                var openRouterMessages = new List<object>
        {
            new { role = "system", content = "You are a helpful assistant." }
        };

                openRouterMessages.AddRange(history
                    .Where(m => m.Role == "User" || m.Role == "AI")
                    .Select(m => new
                    {
                        role = m.Role == "User" ? "user" : "assistant",
                        content = m.Message
                    }));

                // Debug logging for prompt sent to OpenRouter
                _logger.LogInformation("Sending to OpenRouter:\n{0}",
                    JsonSerializer.Serialize(openRouterMessages, new JsonSerializerOptions { WriteIndented = true }));

                // Send request to OpenRouter
                var requestBody = new { model, messages = openRouterMessages };
                var response = await client.PostAsync(
                    "https://openrouter.ai/api/v1/chat/completions",
                    new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
                );

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"OpenRouter API error: {response.StatusCode} - {errorContent}");
                    return StatusCode(500, new { message = "AI service unavailable" });
                }

                var content = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(content);
                var choices = jsonDoc.RootElement.GetProperty("choices");

                if (choices.GetArrayLength() == 0)
                {
                    return StatusCode(500, new { message = "No response from AI" });
                }

                var aiMessage = choices[0].GetProperty("message").GetProperty("content").GetString() ?? "No response";

                // Save AI response
                await _chatHistoryService.AddMessagesAsync(chatId, new List<MessageDTO> {
            new MessageDTO { Role = "AI", Message = aiMessage }
        });

                // Return result
                return Ok(new
                {
                    message = aiMessage,
                    chatId = chatId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Complete endpoint");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

    }
}
