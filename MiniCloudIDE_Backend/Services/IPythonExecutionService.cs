namespace MiniCloudIDE_Backend.Services
{
    public interface IPythonExecutionService
    {
        Task<(string output, string errors)> ExecuteAsync(string code, CancellationToken cancellationToken = default);
    }
}
