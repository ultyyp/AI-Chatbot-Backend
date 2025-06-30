using AI_Chatbot_Backend.Data;
using AI_Chatbot_Backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// Register DbContext
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register chat history service
builder.Services.AddScoped<IChatHistoryService, ChatHistoryService>();

// CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("NuxtApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("NuxtApp");
app.UseAuthorization();
app.MapControllers();
app.Run();
