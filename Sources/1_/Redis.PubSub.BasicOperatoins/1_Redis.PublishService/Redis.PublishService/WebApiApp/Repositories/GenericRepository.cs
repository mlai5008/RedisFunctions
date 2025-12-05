using CommonResources.Models;
using CommonResources.Services;
using Microsoft.EntityFrameworkCore;
using WebApiApp.Context;
using WebApiApp.Repositories.Interfaces;
using WebApiApp.Services.Interfaces;

namespace WebApiApp.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly ILogger<GenericRedisMessageBrokerService<Product>> _logger;
        private readonly ApplicationContext _context;
        private readonly DbSet<TEntity> _dbSet;
        private readonly IGenericRedisCacheService<Product> _genericRedisCacheService;

        public GenericRepository(ILogger<GenericRedisMessageBrokerService<Product>> logger, ApplicationContext context, IGenericRedisCacheService<Product> genericRedisCacheService)
        {
            _logger = logger;
            _context = context;
            _dbSet = context.Set<TEntity>();
            _genericRedisCacheService = genericRedisCacheService;
        }       

        public async Task<IReadOnlyList<TEntity>> GetAllAsinc()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<TEntity> GetByIdAsinc(Guid id)
        {       
            var productCache = await _genericRedisCacheService.GetValueAsync<TEntity>(id.ToString());
            if (productCache != null)
            {
                return productCache;
            }

            var productDB = await _dbSet.FindAsync(id);

            if (productDB != null)
            {
                _logger.LogInformation($"-/- {typeof(GenericRepository<>).Name}. Item [{id}] find to DB. {DateTimeOffset.Now}");
                await _genericRedisCacheService.SetValueAsync<TEntity>(id.ToString(), productDB);
            }

            return productDB;
        }

        public async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsinc(id);
            _context.Set<TEntity>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
