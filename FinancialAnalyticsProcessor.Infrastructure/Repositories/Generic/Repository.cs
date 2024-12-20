using FinancialAnalyticsProcessor.Domain.Interfaces.Repositories;
using FinancialAnalyticsProcessor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace FinancialAnalyticsProcessor.Infrastructure.Repositories.Generic
{
    public class  Repository<T> : IRepository<T> where T : class
    {
        private readonly TransactionDbContext _context;
        private readonly DbSet<T> _dbSet;

        private readonly ILogger<Repository<T>> _logger;

        public Repository(TransactionDbContext context, ILogger<Repository<T>> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task BulkInsertAsync(IEnumerable<T> entities)
        {
            _logger.LogInformation("Starting bulk insert for {EntityType}.", typeof(T).Name);

            try
            {
                var entityCount = entities.Count();
                _logger.LogInformation("{EntityCount} entities detected for bulk insertion.", entityCount);

                await _dbSet.AddRangeAsync(entities); // Add entities in bulk
                await _context.SaveChangesAsync();    // Save changes to the database

                _logger.LogInformation("Successfully inserted {EntityCount} entities into {EntityType}.", entityCount, typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while performing bulk insert for {EntityType}.", typeof(T).Name);
                throw; // Re-throw the exception to propagate it up the call stack
            }
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Retrieving entity with ID {Id} from the database.", id);
            try
            {
                _logger.BeginScope("Method: GetByIdAsync");


                IQueryable<T> query = _context.Set<T>();


                var entityType = _context.Model.FindEntityType(typeof(T));
                if (entityType != null)
                {
                    foreach (var navigation in entityType.GetNavigations())
                    {
                        query = query.Include(navigation.Name);
                    }
                }

                var entity = await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);

                if (entity == null)
                {
                    _logger.LogWarning("Entity with ID {Id} not found.", id);
                }
                else
                {
                    _logger.LogInformation("Entity with ID {Id} retrieved successfully.", id);
                }

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving entity with ID {Id}.", id);
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            _logger.LogInformation("Retrieving all entities with related data from the database.");
            try
            {
                using (_logger.BeginScope("Method: GetAllAsync"))
                {
                    IQueryable<T> query = _context.Set<T>();


                    if (includes != null)
                    {
                        foreach (var include in includes)
                        {
                            query = query.Include(include);
                        }
                    }

                    var entities = await query.ToListAsync();
                    _logger.LogInformation("Successfully retrieved {Count} entities.", entities.Count);
                    return entities;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving entities.");
                throw;
            }
        }

        public async Task AddAsync(T entity)
        {
            _logger.LogInformation("Adding a new entity to the database: {Entity}.", entity);
            try
            {
                using (_logger.BeginScope("Method: AddAsync"))
                {
                    using var dbContext = _context;
                    await dbContext.Set<T>().AddAsync(entity);
                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation("Entity added successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the entity: {Entity}.", entity);
                throw;
            }
        }
        public async Task UpdateAsync(T entity)
        {
            _logger.LogInformation("Updating an entity in the database: {Entity}.", entity);

            try
            {
                using (_logger.BeginScope("Method: UpdateAsync"))
                {
                    using var dbContext = _context;

                    // Detach any existing tracked entity with the same key
                    var existingEntity = dbContext.Set<T>().Local.FirstOrDefault(e => ((dynamic)e).Id == ((dynamic)entity).Id);
                    if (existingEntity != null)
                    {
                        dbContext.Entry(existingEntity).State = EntityState.Detached;
                    }

                    // Attach the updated entity and mark it as modified
                    dbContext.Attach(entity);
                    dbContext.Entry(entity).State = EntityState.Modified;

                    // Save changes
                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation("Entity updated successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the entity: {Entity}.", entity);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting entity with ID {Id} from the database.", id);
            try
            {
                using (_logger.BeginScope("Method: DeleteAsync"))
                {
                    using var dbContext = _context;
                    var entity = await dbContext.Set<T>().FindAsync(id);
                    if (entity == null)
                    {
                        _logger.LogWarning("Entity with ID {Id} not found. Deletion skipped.", id);
                        return false; // Return false if the entity was not found
                    }

                    dbContext.Set<T>().Remove(entity);
                    await dbContext.SaveChangesAsync();

                    _logger.LogInformation("Entity with ID {Id} deleted successfully.", id);
                    return true; // Return true if the deletion was successful
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the entity with ID {Id}.", id);
                return false; // Return false if an exception occurred
            }
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            _logger.LogInformation("Searching for entities with a given predicate.");
            try
            {
                using (_logger.BeginScope("Method: FindAsync"))
                {
                    using var dbContext = _context;
                    var results = await dbContext.Set<T>().Where(predicate).ToListAsync();
                    _logger.LogInformation("Successfully found {Count} matching entities.", results.Count);
                    return results;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for entities.");
                throw;
            }
        }

    }
}
