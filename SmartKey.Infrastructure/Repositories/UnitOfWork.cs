using Microsoft.EntityFrameworkCore;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using System.Collections;

namespace SmartKey.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        private Hashtable _repositories = new();

        public UnitOfWork(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy hoặc khởi tạo repository cho một entity cụ thể.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TId"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IRepository<TEntity, TId> GetRepository<TEntity, TId>()
            where TEntity : Entity<TId>
        {
            var typeName = typeof(TEntity).Name;

            if (!_repositories.ContainsKey(typeName))
            {
                var repositoryType = typeof(Repository<,>).MakeGenericType(typeof(TEntity), typeof(TId));
                var repositoryInstance = Activator.CreateInstance(repositoryType, _context);
                _repositories.Add(typeName, repositoryInstance);
            }

            return (IRepository<TEntity, TId>)_repositories[typeName]!;
        }

        /// <summary>
        /// Ghi tất cả thay đổi của context xuống database.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            foreach (var entry in _context.ChangeTracker.Entries<IAuditable>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    var hasRealChanges = entry.Properties
                        .Any(p => p.IsModified && p.Metadata.Name != nameof(IAuditable.UpdatedAt));

                    if (hasRealChanges)
                    {
                        entry.Entity.UpdatedAt = now;
                    }
                    else
                    {
                        entry.Property(nameof(IAuditable.UpdatedAt)).IsModified = false;
                    }
                }
            }

            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Giải phóng tài nguyên
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
