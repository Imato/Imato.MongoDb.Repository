using MongoDB.Bson.Serialization.Attributes;

namespace Imato.MongoDb.Repository
{
    public class BaseEintity : IEntity
    {
        public string Id { get; set; } = string.Empty;

        [WithIndex]
        public string Name { get; set; } = string.Empty;
    }
}