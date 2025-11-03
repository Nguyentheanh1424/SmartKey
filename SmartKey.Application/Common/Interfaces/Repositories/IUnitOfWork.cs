using SmartKey.Domain.Common;

namespace SmartKey.Application.Common.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity, TId> GetRepository<TEntity, TId>()
            where TEntity : Entity<TId>;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
