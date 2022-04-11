namespace Imato.MongoDb.Repository
{
    public interface ILogEntry : IEntity
    {
        DateTime Date { get; set; }
        LogLevel Level { get; set; }
        string? Message { get; set; }
    }
}