namespace Imato.MongoDb.Repository
{
    public class MongoConfiguration
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
        public string DataBase { get; set; } = "test";

        /// <summary>
        /// Delete documents forever or mark it as Deleted
        /// </summary>
        public bool DeleteForever { get; set; }

        /// <summary>
        /// Use custom id generator. ObjectId is default
        /// </summary>
        public DocumentIdGenerator IdGenerator { get; set; } = DocumentIdGenerator.ObjectId;

        /// <summary>
        /// Used with IdGenerator = ShortString
        /// </summary>
        public ushort IdLength { get; set; } = 5;

        public int LogCollectionSIze { get; set; } = 1024000;

        /// <summary>
        /// Procedures for db initialization
        /// </summary>
        public List<Procedure> InitProcedures { get; set; } = new List<Procedure>();
    }

    public enum DocumentIdGenerator
    {
        ObjectId, ShortString, Long, Guid
    }
}