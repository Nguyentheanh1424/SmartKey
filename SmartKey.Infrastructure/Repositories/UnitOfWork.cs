using Microsoft.EntityFrameworkCore;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using System.Collections;

namespace SmartKey.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        private Hashtable _repositories = new();
        private readonly ICurrentUserService _currentUser;

        public UnitOfWork(DbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
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
            var userId = _currentUser.UserId;

            foreach (var entry in _context.ChangeTracker.Entries<IHasCreationTime>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;

                    if (entry.Entity.CreatedBy == null)
                        entry.Entity.CreatedBy = userId;

                    if (entry.Entity is IHasModificationTime mod)
                    {
                        mod.UpdatedAt = now;
                        mod.UpdatedBy = userId;
                    }
                }
            }

            foreach (var entry in _context.ChangeTracker.Entries<IHasModificationTime>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;

                    if (entry.Entity is IHasModificationTime mod)
                        mod.UpdatedBy = userId;
                }
            }

            foreach (var entry in _context.ChangeTracker.Entries<ISoftDelete>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;

                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    entry.Entity.DeletedBy = userId;
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
