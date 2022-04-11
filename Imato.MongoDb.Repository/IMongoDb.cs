using MongoDB.Driver;
using System.Linq.Expressions;

namespace Imato.MongoDb.Repository
{
    public interface IMongoDb
    {
        /// <summary>
        /// Collection objects T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IMongoCollection<T> GetCollection<T>() where T : IEntity;

        /// <summary>
        /// Base or custome repository for objects T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IRepository<T> GetRepository<T>() where T : IEntity;

        /// <summary>
        /// View db Configuration
        /// </summary>
        MongoConfiguration Configuration { get; }

        /// <summary>
        /// Drop and create db
        /// </summary>
        void Clean();

        /// <summary>
        /// Get value using default repository of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">Search expression</param>
        /// <returns>value T or default</returns>
        Task<T?> GetValueAsync<T>(Expression<Func<T, bool>> expression) where T : IEntity;

        /// <summary>
        /// Get values using default repository of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">Search expression</param>
        /// <returns>value IEnumerable<T> or empty</returns>
        Task<IEnumerable<T>> GetValuesAsync<T>(Expression<Func<T, bool>> expression) where T : IEntity;

        /// <summary>
        /// Create or update value using default repository of T
        /// </summary>
        /// <typeparam doc="T"></typeparam>
        /// <param name="doc">Object T</param>
        Task<T> CreateOrUpdateAsync<T>(T doc) where T : IEntity;

        /// <summary>
        /// Save log
        /// </summary>
        /// <param name="logEntry"></param>
        /// <returns></returns>
        Task WriteLogAsync(ILogEntry logEntry);

        /// <summary>
        /// Save logs
        /// </summary>
        /// <param name="logs">Log array</param>
        /// <returns></returns>
        Task WriteLogsAsync(IEnumerable<ILogEntry> logs);

        /// <summary>
        /// Get logs from db
        /// </summary>
        /// <param name="count"></param>
        /// <param name="page"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        Task<IEnumerable<ILogEntry>> GetLogsAsync(int count = 100, int page = 1, LogLevel? level = null);

        /// <summary>
        /// Save single parameter T value in db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="name">Save with this name or use nameof(T)</param>
        Task SetParameterAsync<T>(T value, string? name = null);

        /// <summary>
        /// Save single parameter T value in db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="name">Save with this name or use nameof(T)</param>
        Task SetParameterAsync<T>(IEnumerable<T> values, string? name = null);

        /// <summary>
        /// Get single parameter T value from db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Search by this name</param>
        /// <returns>T</returns>
        Task<T?> GetParameterAsync<T>(string? name = null) where T : class;

        /// <summary>
        /// Get single parameter IEnumerable<T> values from db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Search by this name</param>
        /// <returns>IEnumerable<T></returns>
        Task<IEnumerable<T>> GetParameterValuesAsync<T>(string? name = null) where T : class;
    }
}