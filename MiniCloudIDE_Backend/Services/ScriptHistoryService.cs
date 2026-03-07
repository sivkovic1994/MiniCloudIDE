using Microsoft.EntityFrameworkCore;
using MiniCloudIDE_Backend.Data;
using MiniCloudIDE_Backend.Models;

namespace MiniCloudIDE_Backend.Services
{
    public class ScriptHistoryService : IScriptHistoryService
    {
        private readonly AppDbContext _context;

        public ScriptHistoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveScript(string language, string code)
        {
            ScriptHistory history = new ScriptHistory
            {
                Language = language,
                Code = code,
                CreatedAt = DateTime.UtcNow
            };

            _context.ScriptHistories.Add(history);

            await _context.SaveChangesAsync();
        }

        public async Task<List<ScriptHistory>> GetHistory(string language)
        {
            return await _context.ScriptHistories
                .Where(h => h.Language == language)
                .OrderByDescending(h => h.CreatedAt)
                .Take(10)
                .ToListAsync();
        }

        public async Task<ScriptHistory?> GetScriptById(int id)
        {
            return await _context.ScriptHistories
                .FirstOrDefaultAsync(h => h.Id == id);
        }
    }
}