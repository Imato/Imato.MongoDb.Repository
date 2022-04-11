namespace Imato.MongoDb.Repository
{
    public class LogEntry : ILogEntry
    {
        public string? Id { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public LogLevel Level { get; set; } = LogLevel.Info;
        public string? Source { get; set; }
        public string? Message { get; set; }
        public string? StackTrace { get; set; }
        public object? Parameters { get; set; }
    }

    public enum LogLevel
    {
        Debug = 0, Info, Warning, Error
    }
}