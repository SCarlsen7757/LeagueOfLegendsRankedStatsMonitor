using InfluxDbDataInsert.Dto;
using Microsoft.Extensions.Options;

namespace LeagueOfLegendsInFluxTelegrafAgent.Services
{
    public class RiotGamesAccountServiceOptions
    {
        public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromHours(24);
    }

    public class RiotGamesAccountService : BackgroundService, IRiotGamesAccountService
    {
        private readonly RiotGamesAccountServiceOptions options;
        private readonly ILogger<RiotGamesAccountService> logger;
        private readonly IRiotGamesApiService apiService;

        public event Action<Dictionary<string, AccountDto>>? OnNewAccountData;


        public Dictionary<string, AccountDto> Accounts { get; private set; } = [];

        public RiotGamesAccountService(IOptions<RiotGamesAccountServiceOptions> options,
                              ILogger<RiotGamesAccountService> logger,
                              IRiotGamesApiService apiService)
        {
            this.options = options.Value;
            this.logger = logger;
            this.apiService = apiService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (await FetchAllAccountDataAsync()) OnNewAccountData?.Invoke(Accounts);
                await Task.Delay(options.RefreshInterval, stoppingToken);
            }
        }

        public async Task<AccountDto?> FetchAccountByRiotIdAsync(string gameName, string tagLine)
        {
            RiotGamesRegions region = apiService.GetRegionsFromTagLine(tagLine);
            string url = $"{apiService.GetUrl(region)}/riot/account/v1/accounts/by-riot-id/{gameName}/{tagLine}";
            return await apiService.GetAsync<AccountDto>(url);
        }

        public bool AddAccount(AccountDto account)
        {
            if (Accounts.ContainsKey(account.PuuId))
            {
                return false;
            }

            Accounts[account.PuuId] = account;
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Added account: {GameName}#{TagLine} (PUUID: {PuuId})",
                    account.GameName, account.TagLine, account.PuuId);

            OnNewAccountData?.Invoke(Accounts);
            return true;
        }

        public bool RemoveAccount(string puuId)
        {
            if (Accounts.Remove(puuId))
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("Removed account with PUUID: {PuuId}", puuId);
                return true;
            }
            return false;
        }

        private async Task<bool> FetchAllAccountDataAsync()
        {
            bool newDataFetched = false;
            foreach (var keyPair in Accounts)
            {
                var accountData = await FetchAccountDataAsync(keyPair.Key, keyPair.Value.TagLine);
                if (accountData != null)
                {
                    if (accountData.GameName == keyPair.Value.GameName && accountData.TagLine == keyPair.Value.TagLine) continue;

                    Accounts[keyPair.Key] = new AccountDto
                    {
                        PuuId = accountData.PuuId,
                        GameName = accountData.GameName,
                        TagLine = accountData.TagLine,
                    };

                    newDataFetched = true;
                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("Updated account data for PUUID: {Puuid}", keyPair);
                }
            }

            if (newDataFetched)
            {
                OnNewAccountData?.Invoke(Accounts);
                newDataFetched = false;
            }
            return newDataFetched;
        }

        private async Task<AccountDto?> FetchAccountDataAsync(string puuid, string tagLine)
        {
            RiotGamesRegions regions = apiService.GetRegionsFromTagLine(tagLine);

            string url = $"{apiService.GetUrl(regions)}/riot/account/v1/accounts/by-puuid/{puuid}";
            return await apiService.GetAsync<AccountDto>(url);
        }
    }
}
