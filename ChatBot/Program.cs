using ChatBot.Applications.Plugins;
using ChatBot.Domain.Chats;
using ChatBot.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseInMemoryDatabase("InMemoryDb"));
builder.Services.AddDbContext<AppDbContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



// builder.Services.AddOpenAIChatCompletion("gpt-3.5-turbo", new Uri("https://api.avalai.ir/v1"), "aa-P4dn4HN5uMyBt09IRvtcfSxXD37DSSGQPw7skt4ONh3K6tCe");
builder.Services.AddOllamaChatCompletion("llama3.1", new HttpClient()
{
    BaseAddress = new Uri("http://localhost:11434"),
    Timeout = TimeSpan.FromMinutes(5)
});


builder.Services.AddTransient<Kernel>(serviceProvider =>
{
    var kernel = new Kernel(serviceProvider);
    kernel.Plugins.AddFromType<HistoryPlugin>("History", serviceProvider);

    return kernel;
});

// builder.Services.AddSingleton<IFunctionInvocationFilter, LoggingFilter>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();


app.MapGet("/api/Chat", async (string prompt, Kernel kernel, AppDbContext _dbContext) =>
{
    var chatService = kernel.GetRequiredService<IChatCompletionService>();
    var history =new ChatHistory();

    // var listHistories = await _dbContext.UserHistories.ToListAsync();
    
    // if(listHistories.Any())
    //     listHistories.ForEach(x=> history.AddUserMessage(x.Message));
    
    history.AddUserMessage(prompt);

    _dbContext.UserHistories.Add(new UserHistory()
    {
        Id = Guid.NewGuid(), Message = prompt,
    });
    
    var message = await chatService.GetChatMessageContentAsync(chatHistory: history,
        executionSettings: new()
        {
            FunctionChoiceBehavior = RequiredFunctionChoiceBehavior.Required()
        },
        kernel: kernel);

    await _dbContext.SaveChangesAsync();
    return Results.Ok(message.Content);

});
app.Run();

