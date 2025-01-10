using System.ComponentModel;
using ChatBot.Domain.Chats;
using ChatBot.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

namespace ChatBot.Applications.Plugins;

public class HistoryPlugin(AppDbContext _dbContext)
{
    [KernelFunction(nameof(GetAllHistories))]
    [Description("return all history of chats and memory")]
    
    public async Task<List<UserHistory>> GetAllHistories()
    {
        return await _dbContext.UserHistories
            .IgnoreAutoIncludes()
            .ToListAsync();
    }
}