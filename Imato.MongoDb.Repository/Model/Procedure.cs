namespace Imato.MongoDb.Repository
{
    public class Procedure
    {
        public int Id { get; set; }
        public Action? Action { get; set; }
        public bool RunEveryTime { get; set; }
        public bool IsDone { get; set; }
    }
}