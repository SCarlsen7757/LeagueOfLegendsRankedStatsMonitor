using LeagueOfLegendsInFluxTelegrafAgent.Dto;
using LeagueOfLegendsInFluxTelegrafAgent.Dto.RiotGames;
using LeagueOfLegendsInFluxTelegrafAgent.Services.Interfaces;
using LeagueOfLegendsInFluxTelegrafAgent.Services.RiotGames.Interfaces;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace LeagueOfLegendsInFluxTelegrafAgent.Services.RiotGames
{
    public class AccountService : BackgroundService, IAccountService
    {
        private readonly AccountServiceOptions options;
        private readonly ILogger<AccountService> logger;
        private readonly IApiService apiService;
        private readonly IAccountStorageService storageService;

        public event Action<IReadOnlyDictionary<string, IAccount>>? OnNewAccountData;

        private readonly ConcurrentDictionary<string, IAccount> accounts = new();

        public IReadOnlyDictionary<string, IAccount> Accounts => accounts;

        public AccountService(IOptions<AccountServiceOptions> options,
                              ILogger<AccountService> logger,
                              IApiService apiService,
                              IAccountStorageService storageService)
        {
            this.options = options.Value;
            this.logger = logger;
            this.apiService = apiService;
            this.storageService = storageService;
        }

        public async Task InitializeAsync()
        {
            foreach (var account in await storageService.GetAllAccountsAsync())
            {
                accounts[account.PuuId] = account;
            }

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Loaded {Count} accounts from storage", Accounts.Count);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitializeAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                if (await FetchAllAccountDataAsync()) OnNewAccountData?.Invoke(Accounts);
                await Task.Delay(options.RefreshInterval, stoppingToken);
            }
        }

        public async Task<AccountDto?> FetchAccountByNameAndTagAsync(string gameName, string tagLine)
        {
            string url = $"{apiService.GetUrl(apiService.Region)}/riot/account/v1/accounts/by-riot-id/{gameName}/{tagLine}";
            return await apiService.GetAsync<AccountDto>(url);
        }

        public async Task<AccountDto?> FetchAccountByPuuidAsync(string puuid)
        {
            string url = $"{apiService.GetUrl(apiService.Region)}/riot/account/v1/accounts/by-puuid/{puuid}";
            return await apiService.GetAsync<AccountDto>(url);
        }

        public bool AddAccount(IAccount account)
        {
            if (Accounts.ContainsKey(account.PuuId))
            {
                return false;
            }

            accounts[account.PuuId] = account;
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Added account: {GameName}#{TagLine} (PUUID: {PuuId})",
                    account.GameName, account.TagLine, account.PuuId);

            _ = storageService.UpsertAccountAsync(account);

            OnNewAccountData?.Invoke(Accounts);
            return true;
        }

        public bool RemoveAccount(string puuId)
        {
            if (accounts.TryRemove(puuId, out var removed))
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("Removed account with PUUID: {PuuId}", puuId);

                _ = storageService.DeleteAccountAsync(puuId);

                OnNewAccountData?.Invoke(Accounts);
                return true;
            }
            return false;
        }

        private async Task<bool> FetchAllAccountDataAsync()
        {
            bool newDataFetched = false;
            foreach (var keyPair in accounts)
            {
                var accountData = await FetchAccountByPuuidAsync(keyPair.Value.PuuId);
                if (accountData != null)
                {
                    if (accountData.GameName == keyPair.Value.GameName && accountData.TagLine == keyPair.Value.TagLine) continue;

                    accounts[keyPair.Key].GameName = accountData.GameName;
                    accounts[keyPair.Key].TagLine = accountData.TagLine;

                    _ = storageService.UpsertAccountAsync(accounts[keyPair.Key]);

                    newDataFetched = true;
                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("Updated account data for PUUID: {Puuid}", keyPair.Value.PuuId);
                }
            }

            if (newDataFetched)
            {
                OnNewAccountData?.Invoke(accounts);
            }
            return newDataFetched;
        }

    }
}
