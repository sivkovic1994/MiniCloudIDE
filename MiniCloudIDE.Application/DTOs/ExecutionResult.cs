namespace MiniCloudIDE.Application.DTOs
{
    public class ExecutionResult
    {
        public string Output { get; set; } = "";
        public string? Errors { get; set; }
        public double ExecutionTimeMs { get; set; }
    }
}
