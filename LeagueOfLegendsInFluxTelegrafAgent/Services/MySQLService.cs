using LeagueOfLegendsInFluxTelegrafAgent.Dto.RiotGames;
using LeagueOfLegendsInFluxTelegrafAgent.Enums.RiotGames;
using LeagueOfLegendsInFluxTelegrafAgent.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LeagueOfLegendsInFluxTelegrafAgent.Services
{
    public class MySQLServiceOptions
    {
        public required string ConnectionString { get; set; }
    }

    public class AccountDbContext : DbContext
    {
        public DbSet<AccountEntity> Accounts { get; set; }

        public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountEntity>(entity =>
            {
                entity.HasKey(e => e.PuuId);
                entity.Property(e => e.PuuId).HasMaxLength(78);
                entity.Property(e => e.GameName).HasMaxLength(255).IsRequired();
                entity.Property(e => e.TagLine).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Platform).IsRequired();
                entity.HasIndex(e => e.PuuId).IsUnique();
            });
        }
    }

    public class AccountEntity
    {
        public required string PuuId { get; set; }
        public required string GameName { get; set; }
        public required string TagLine { get; set; }
        public Platforms Platform { get; set; }

        public AccountDto ToDto()
        {
            return new AccountDto
            {
                PuuId = PuuId,
                GameName = GameName,
                TagLine = TagLine,
                Platform = Platform
            };
        }

        public static AccountEntity FromDto(AccountDto dto)
        {
            return new AccountEntity
            {
                PuuId = dto.PuuId,
                GameName = dto.GameName,
                TagLine = dto.TagLine,
                Platform = dto.Platform
            };
        }
    }

    public class MySQLService : IAccountStorageService
    {
        private readonly ILogger<MySQLService> logger;
        private readonly IDbContextFactory<AccountDbContext> contextFactory;

        public MySQLService(IDbContextFactory<AccountDbContext> contextFactory, ILogger<MySQLService> logger)
        {
            this.logger = logger;
            this.contextFactory = contextFactory;

            // Ensure database is created
            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        public async Task<AccountDto?> GetAccountAsync(string puuId)
        {
            try
            {
                using var dbContext = contextFactory.CreateDbContext();
                var entity = await dbContext.Accounts.FindAsync(puuId);
                return entity?.ToDto();
            }
            catch (Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(ex, "Error retrieving account with PUUID: {PuuId}", puuId);
                return null;
            }
        }

        public async Task<List<AccountDto>> GetAllAccountsAsync()
        {
            try
            {
                using var dbContext = contextFactory.CreateDbContext();
                var entities = await dbContext.Accounts.ToListAsync();
                return entities.Select(e => e.ToDto()).ToList();
            }
            catch (Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(ex, "Error retrieving all accounts from MySQL");
                return [];
            }
        }

        public async Task<bool> UpsertAccountAsync(AccountDto account)
        {
            try
            {
                using var dbContext = contextFactory.CreateDbContext();
                var existingEntity = await dbContext.Accounts.FindAsync(account.PuuId);

                if (existingEntity != null)
                {
                    // Update existing
                    existingEntity.GameName = account.GameName;
                    existingEntity.TagLine = account.TagLine;
                    existingEntity.Platform = account.Platform;
                    dbContext.Accounts.Update(existingEntity);
                }
                else
                {
                    // Insert new
                    var newEntity = AccountEntity.FromDto(account);
                    await dbContext.Accounts.AddAsync(newEntity);
                }

                await dbContext.SaveChangesAsync();

                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("Upserted account in MySQL: {GameName}#{TagLine} (PUUID: {PuuId})",
                        account.GameName, account.TagLine, account.PuuId);

                return true;
            }
            catch (Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(ex, "Error upserting account: {GameName}#{TagLine} (PUUID: {PuuId})",
                        account.GameName, account.TagLine, account.PuuId);
                return false;
            }
        }

        public async Task<bool> DeleteAccountAsync(string puuId)
        {
            try
            {
                using var dbContext = contextFactory.CreateDbContext();
                var entity = await dbContext.Accounts.FindAsync(puuId);

                if (entity != null)
                {
                    dbContext.Accounts.Remove(entity);
                    await dbContext.SaveChangesAsync();

                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("Deleted account from MySQL with PUUID: {PuuId}", puuId);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(ex, "Error deleting account with PUUID: {PuuId}", puuId);
                return false;
            }
        }
    }
}
