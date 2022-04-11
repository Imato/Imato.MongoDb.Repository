using MongoDB.Driver;
using Imato.JsonConfiguration;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson;

namespace Imato.MongoDb.Repository
{
    public class MongoDb : IMongoDb
    {
        private IMongoDatabase db = null!;
        private MongoClient client = null!;
        private MongoConfiguration config = null!;
        private ConcurrentDictionary<string, object> repositories = new ConcurrentDictionary<string, object>();
        private bool configured;
        private IMongoCollection<ILogEntry> logCollection = null!;
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public void Configure(MongoConfiguration? cfg = null)
        {
            semaphore.Wait(60000);
            if (!configured)
            {
                config = cfg ?? Configuration<MongoConfiguration>.Get();
                var settings = MongoClientSettings.FromConnectionString(config.ConnectionString);
                settings.ServerApi = new ServerApi(ServerApiVersion.V1);
                client = new MongoClient(settings);
                db = client.GetDatabase(config.DataBase);

                // Logs
                if (!CollectionExits("Logs"))
                {
                    db.CreateCollection("Logs", new CreateCollectionOptions
                    {
                        Capped = true,
                        MaxSize = config.LogCollectionSIze
                    });
                }
                logCollection = db.GetCollection<ILogEntry>("Logs");

                Init();
                configured = true;
            }
            semaphore.Release();
        }

        private void Init()
        {
            foreach (var procedure in config.InitProcedures)
            {
                Run(procedure);
            }
        }

        public MongoConfiguration Configuration => config;

        private string Name<T>()
        {
            return $"{typeof(T).Name}s";
        }

        private bool CollectionExits(string name)
        {
            var filter = new BsonDocument("name", name);
            var options = new ListCollectionNamesOptions
            {
                Filter = filter
            };
            return db.ListCollectionNames(options).Any();
        }

        private IMongoCollection<T> CreateCollection<T>() where T : IEntity
        {
            /*
            BsonClassMap.RegisterClassMap<T>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
            });
            */

            var coll = db.GetCollection<T>(Name<T>());
            CreateIndex(coll, "Name");
            return coll;
        }

        private void CreateIndex<T>(IMongoCollection<T> coll, string field, bool unique = false)
        {
            var indexBuilder = Builders<T>.IndexKeys;
            var keys = indexBuilder.Ascending(field);
            var options = new CreateIndexOptions
            {
                Name = $"{nameof(T)}_{field}",
                Unique = unique
            };
            var indexModel = new CreateIndexModel<T>(keys, options);
            coll.Indexes.CreateOne(indexModel);
        }

        public IMongoCollection<T> GetCollection<T>() where T : IEntity
        {
            return CreateCollection<T>();
        }

        public void Run(Procedure procedure)
        {
            var coll = db.GetCollection<Procedure>("InitProcedures");
            var prc = coll.Find(x => x.Id == procedure.Id).FirstOrDefault() ?? procedure;
            if (!prc.IsDone || procedure.RunEveryTime)
            {
                if (prc.Action != null) prc.Action();
                prc.IsDone = true;
                coll.ReplaceOne(x => x.Id == prc.Id,
                    prc,
                    new ReplaceOptions
                    {
                        IsUpsert = true,
                    });
            }
        }

        public async Task<IEnumerable<T>> GetValuesAsync<T>(Expression<Func<T, bool>> expression) where T : IEntity
        {
            var repo = GetRepository<T>();
            return await repo.FindAllAsync(expression);
        }

        public async Task<T?> GetValueAsync<T>(Expression<Func<T, bool>> expression) where T : IEntity
        {
            return (await GetValuesAsync<T>(expression)).FirstOrDefault();
        }

        public async Task<T> CreateOrUpdateAsync<T>(T doc) where T : IEntity
        {
            var repo = GetRepository<T>();
            return await repo.CreateOrUpdateAsync(doc);
        }

        public void Clean()
        {
            client.DropDatabase(config.DataBase);
        }

        public IRepository<T> GetRepository<T>() where T : IEntity
        {
            var name = Name<T>();
            if (!repositories.ContainsKey(name))
            {
                repositories[name] = new BaseRepository<T>(this);
            }
            return repositories[name] as IRepository<T>
                ?? throw new ArgumentOutOfRangeException(name);
        }

        public async Task WriteLogAsync(ILogEntry logEntry)
        {
            await logCollection
                .WithWriteConcern(WriteConcern.W1)
                .InsertOneAsync(logEntry);
        }

        public async Task WriteLogsAsync(IEnumerable<ILogEntry> logs)
        {
            foreach (var log in logs)
            {
                log.Id = ObjectId.GenerateNewId().ToString();
            }
            await logCollection
                .WithWriteConcern(WriteConcern.W1)
                .InsertManyAsync(logs);
        }

        public async Task<IEnumerable<ILogEntry>> GetLogsAsync(int count = 100, int page = 1, LogLevel? type = null)
        {
            var query = Builders<ILogEntry>.Filter.Empty;
            if (type != null) query = Builders<ILogEntry>.Filter.Gte("Level", type);
            var sort = Builders<ILogEntry>.Sort.Descending("$natural");
            var r = await logCollection
                .Find(query)
                .Sort(sort)
                .Skip((page - 1) * count)
                .Limit(count)
                .ToListAsync();
            return r;
        }

        private string GetParameterName<T>(string? name)
        {
            return name ?? typeof(T).Name;
        }

        public async Task SetParameterAsync<T>(T value, string? name = null)
        {
            var parameterName = GetParameterName<T>(name);
            var repo = GetRepository<Parameter>();
            var parameter = (await repo.FindAsync(x => x.Name == parameterName)) ?? new Parameter
            {
                Id = repo.NewId(),
                Name = parameterName,
                Value = value
            };
            parameter.Value = value;
            await repo.CreateOrUpdateAsync(parameter);
        }

        public async Task SetParameterAsync<T>(IEnumerable<T> values, string? name = null)
        {
            var parameterName = $"{GetParameterName<T>(name)}s";
            await SetParameterAsync<IEnumerable<T>>(values, parameterName);
        }

        public async Task<T?> GetParameterAsync<T>(string? name = null) where T : class
        {
            var parameterName = GetParameterName<T>(name);
            var repo = GetRepository<Parameter>();
            var parameter = (await repo.FindAsync(x => x.Name == parameterName));
            return (parameter?.Value as T) ?? default;
        }

        public async Task<IEnumerable<T>> GetParameterValuesAsync<T>(string? name = null) where T : class
        {
            var parameterName = $"{GetParameterName<T>(name)}s";
            var repo = GetRepository<Parameter>();
            var parameter = (await repo.FindAsync(x => x.Name == parameterName));
            return (parameter?.Value as IEnumerable<T>) ?? Enumerable.Empty<T>();
        }
    }
}