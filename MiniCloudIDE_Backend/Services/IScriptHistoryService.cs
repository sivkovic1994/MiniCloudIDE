using MiniCloudIDE_Backend.Models;

namespace MiniCloudIDE_Backend.Services
{
    public interface IScriptHistoryService
    {
        Task SaveScript(string language, string code);
        Task<List<ScriptHistory>> GetHistory(string language);
        Task<ScriptHistory?> GetScriptById(int id);
    }
}