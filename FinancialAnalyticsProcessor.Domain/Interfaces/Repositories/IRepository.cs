using System.Linq.Expressions;

namespace FinancialAnalyticsProcessor.Domain.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task BulkInsertAsync(IEnumerable<T> entities);
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    }
}
