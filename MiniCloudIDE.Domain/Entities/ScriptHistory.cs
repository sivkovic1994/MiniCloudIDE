namespace MiniCloudIDE.Domain.Entities
{
    public class ScriptHistory
    {
        public int Id { get; set; }
        public required string Language { get; set; }
        public string? Code { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public required string UserId { get; set; }
    }
}
