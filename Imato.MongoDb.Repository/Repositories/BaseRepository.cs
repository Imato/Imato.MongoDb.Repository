using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Imato.MongoDb.Repository
{
    public class BaseRepository<T> : IRepository<T> where T : IEntity
    {
        protected readonly IMongoDb db;
        protected readonly IMongoCollection<T> collection;
        protected long lastId;
        protected SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        protected static FindOptions<T> findOptions = new FindOptions<T>
        {
            MaxAwaitTime = TimeSpan.FromMilliseconds(10),
            BatchSize = 100
        };

        protected static ReplaceOptions replaceOptions = new ReplaceOptions
        {
            IsUpsert = true
        };

        protected static UpdateOptions updateOptions = new UpdateOptions
        {
            IsUpsert = true
        };

        protected static FilterDefinition<T> filterDeleted = Builders<T>.Filter.Exists("Deleted", false);

        public BaseRepository(IMongoDb? mongoDb = null)
        {
            db = mongoDb ?? new MongoDb();
            collection = db.GetCollection<T>();
            if (db.Configuration.IdGenerator == DocumentIdGenerator.Long)
            {
                semaphore.Wait(1000);
                var sort = Builders<T>.Sort.Descending(x => x.Id);
                var id = collection.Find("{ }").Sort(sort).Limit(1).ToList().Select(x => x.Id).FirstOrDefault();
                if (id != null)
                {
                    lastId = long.Parse(id);
                }
                semaphore.Release();
            }
        }

        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>>? expression = null)
        {
            if (expression != null)
            {
                return (await collection.FindAsync(expression & filterDeleted, findOptions)).ToEnumerable();
            }
            return (await collection.FindAsync(filterDeleted, findOptions)).ToEnumerable();
        }

        public async Task<T?> FindAsync(Expression<Func<T, bool>>? expression = null)
        {
            return (await FindAllAsync(expression)).FirstOrDefault();
        }

        public async Task<T> CreateAsync(T doc)
        {
            doc.Id = NewId();
            await collection.InsertOneAsync(doc);
            return doc;
        }

        public async Task CreateAsync(IEnumerable<T> docs)
        {
            foreach (var doc in docs)
            {
                doc.Id = NewId();
            }
            await collection.InsertManyAsync(docs);
        }

        public async Task<T> UpdateAsync(T doc)
        {
            await collection.ReplaceOneAsync(x => x.Id == doc.Id, doc);
            return doc;
        }

        public async Task<T> CreateOrUpdateAsync(T doc)
        {
            if (string.IsNullOrEmpty(doc.Id))
            {
                doc.Id = NewId();
            }
            await collection.ReplaceOneAsync(x => x.Id == doc.Id, doc, replaceOptions);
            return doc;
        }

        public async Task DeleteAsync(T doc)
        {
            if (db.Configuration.DeleteForever)
            {
                await collection.DeleteOneAsync(x => x.Id == doc.Id);
            }
            else
            {
                var update = Builders<T>.Update.Set("Deleted", true);
                await collection.UpdateOneAsync(x => x.Id == doc.Id, update, updateOptions);
            }
        }

        public string NewId()
        {
            switch (db.Configuration.IdGenerator)
            {
                case DocumentIdGenerator.ObjectId:
                    return ObjectId.GenerateNewId().ToString();

                case DocumentIdGenerator.ShortString:
                    var id = StringUtils.NewId(db.Configuration.IdLength);
                    var exists = collection.Find(x => x.Id == id).Any();
                    if (!exists) return id;
                    return NewId();

                case DocumentIdGenerator.Long:
                    semaphore.Wait(1000);
                    var r = lastId++;
                    semaphore.Release();
                    return r.ToString();
            }

            return Guid.NewGuid().ToString();
        }
    }
}