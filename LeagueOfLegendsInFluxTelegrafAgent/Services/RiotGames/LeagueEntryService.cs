using LeagueOfLegendsInFluxTelegrafAgent.Dto.RiotGames;
using LeagueOfLegendsInFluxTelegrafAgent.Enums.RiotGames;
using LeagueOfLegendsInFluxTelegrafAgent.Services.RiotGames.Interfaces;
using Microsoft.Extensions.Options;

namespace LeagueOfLegendsInFluxTelegrafAgent.Services.RiotGames
{
    public class LeagueEntryService : BackgroundService, ILeagueEntryService
    {
        private readonly LeagueEntryServiceOptions options;
        private readonly ILogger<LeagueEntryService> logger;
        private readonly IApiService apiService;
        private readonly IAccountService accountService;

        private static readonly HashSet<LeagueEntryDTO> leagueEntryDTOs = [];

        public event Action<IList<LeagueEntryDTO>>? OnNewLeagueEntryData;
        public Dictionary<string, LeagueEntryDTO> LeagueEntries { get; private set; } = [];

        public LeagueEntryService(IOptions<LeagueEntryServiceOptions> options,
                                  ILogger<LeagueEntryService> logger,
                                  IApiService apiService,
                                  IAccountService accountService)
        {
            this.options = options.Value;
            this.logger = logger;
            this.apiService = apiService;
            this.accountService = accountService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Initial delay to allow other services to start

            while (!stoppingToken.IsCancellationRequested)
            {
                await FetchAllLeagueEntryDataAsync();

                await Task.Delay(options.RefreshInterval, stoppingToken);
            }
        }

        public async Task FetchAllLeagueEntryDataAsync()
        {
            var accountsSnapshot = accountService.Accounts.ToList();

            foreach (var keyPair in accountsSnapshot)
            {
                var accountData = await FetchLeagueEntryDataAsync(keyPair.Key, keyPair.Value.Platform);
                if (accountData == null)
                {
                    if (logger.IsEnabled(LogLevel.Warning))
                        logger.LogWarning("No league entry data found for PUUID: {Puuid}", keyPair.Key);
                    continue;
                }

                if (!LeagueEntries.TryAdd(keyPair.Key, accountData))
                {
                    LeagueEntries[keyPair.Key] = accountData;
                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("Updated account data for PUUID: {Puuid}", keyPair.Key);
                }
                else
                {
                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("Added new account data for PUUID: {Puuid}", keyPair.Key);
                }
            }

            OnNewLeagueEntryData?.Invoke([.. LeagueEntries.Values]);
        }

        public async Task<LeagueEntryDTO?> FetchLeagueEntryDataAsync(string puuid, Platforms platforms)
        {
            string url = $"{apiService.GetUrl(platforms)}/lol/league/v4/entries/by-puuid/{puuid}";
            var result = await apiService.GetAsync<List<LeagueEntryDTO>>(url);
            if (result == null || result.Count == 0)
                return null;
            return result.FirstOrDefault(x => x.QueueType == "RANKED_SOLO_5x5");
        }
    }
}
