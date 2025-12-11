using Microsoft.EntityFrameworkCore;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using System.Linq.Expressions;

namespace SmartKey.Infrastructure.Repositories
{
    public class Repository<TEntity, TId> : IRepository<TEntity, TId> where TEntity : Entity<TId>
    {
        protected readonly DbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public IQueryable<TEntity> GetAll() => _dbSet.AsQueryable();


        public async Task<List<TEntity>> GetAllAsync() =>
            await _dbSet.ToListAsync();


        public async Task<TEntity?> GetByIdAsync(TId id) =>
            await _dbSet.FirstOrDefaultAsync(entity => entity.Id!.Equals(id)!);

        public async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate) =>
            await _dbSet.Where(predicate).ToListAsync();
        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate) =>
            await _dbSet.AnyAsync(predicate);
        public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null) =>
            predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);
        public async Task<TId> AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            return entity.Id;
        }
        public Task UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(TEntity entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }
        public Task DeleteAsync(TId id)
        {
            var entity = _dbSet.FirstOrDefault(e => e.Id!.Equals(id)!);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
            return Task.CompletedTask;
        }

        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }
    }
}
