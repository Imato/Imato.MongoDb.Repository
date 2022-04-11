using Imato.MongoDb.Repository;

namespace Imato.MongoDb.Repository.Test
{
    public static class Utils
    {
        public static IMongoDb GetDb()
        {
            var db = new MongoDb();
            db.Configure(new MongoConfiguration
            {
                DataBase = "testDb"
            });
            db.Clean();
            return db;
        }
    }
}