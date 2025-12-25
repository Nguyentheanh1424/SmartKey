using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Infrastructure.BackgroundJobs
{
    public class DoorCommandTimeoutService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(15);

        public DoorCommandTimeoutService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckTimeoutAsync(stoppingToken);
                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task CheckTimeoutAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var commandRepo = uow.GetRepository<DoorCommand, Guid>();

            var now = DateTime.UtcNow;

            var pendingCommands = await commandRepo.FindAsync(
                c => c.Status == CommandStatus.Pending &&
                     now - c.SentAt > Timeout);

            if (!pendingCommands.Any())
                return;

            foreach (var cmd in pendingCommands)
            {
                cmd.MarkFailed();
            }

            await uow.SaveChangesAsync(ct);
        }
    }
}
