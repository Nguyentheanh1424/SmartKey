using Microsoft.EntityFrameworkCore;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;
using System.Linq.Expressions;

namespace SmartKey.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Define DbSets for your entities here
        public DbSet<User> Users => Set<User>();
        public DbSet<UserAuth> UserAuths => Set<UserAuth>();

        public DbSet<Door> Doors => Set<Door>();
        public DbSet<DoorCommand> DoorCommands => Set<DoorCommand>();
        public DbSet<DoorRecord> DoorRecords => Set<DoorRecord>();
        public DbSet<DoorShare> DoorShares => Set<DoorShare>();

        public DbSet<ICCard> ICCards => Set<ICCard>();
        public DbSet<Passcode> Passcodes => Set<Passcode>();

        public DbSet<MqttInboxMessage> MqttInboxMessages => Set<MqttInboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var isDeletedProperty = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                    var compare = Expression.Equal(isDeletedProperty, Expression.Constant(false));

                    var lambda = Expression.Lambda(compare, parameter);

                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(lambda);
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
