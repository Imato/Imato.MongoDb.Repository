using System.Linq.Expressions;

namespace Imato.MongoDb.Repository
{
    public interface IRepository<T> where T : IEntity
    {
        /// <summary>
        /// List of documents
        /// </summary>
        /// <param name="expression">Search expression or null (all docs)</param>
        /// <returns></returns>
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>>? expression = null);

        /// <summary>
        /// Get one doc
        /// </summary>
        /// <param name="filterExpression">Search expression or null (all docs)</param>
        /// <returns></returns>
        Task<T?> FindAsync(Expression<Func<T, bool>>? filterExpression = null);

        /// <summary>
        /// Create new document with new id
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        Task<T> CreateAsync(T doc);

        /// <summary>
        /// Create new document with new ids
        /// </summary>
        /// <param name="docs"></param>
        /// <returns></returns>
        Task CreateAsync(IEnumerable<T> docs);

        /// <summary>
        /// Replace or create
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>T</returns>
        Task<T> UpdateAsync(T doc);

        /// <summary>
        /// Create new or update exists
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>T</returns>
        Task<T> CreateOrUpdateAsync(T doc);

        /// <summary>
        /// Delete or mark doc Deleted
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        Task DeleteAsync(T doc);

        /// <summary>
        /// Generate new document id
        /// </summary>
        /// <returns></returns>
        string NewId();
    }
}