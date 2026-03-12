using MiniCloudIDE.Application.DTOs;

namespace MiniCloudIDE.Application.Interfaces
{
    public interface ICodeExecutionService
    {
        Task<ExecutionResult> ExecuteAsync(string language, string code);
    }
}
