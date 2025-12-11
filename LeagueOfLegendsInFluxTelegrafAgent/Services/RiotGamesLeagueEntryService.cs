using InfluxDbDataInsert.Dto;
using Microsoft.Extensions.Options;

namespace LeagueOfLegendsInFluxTelegrafAgent.Services
{
    public class RiotGamesLeagueEntryServiceOptions
    {
        public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(30);
    }

    public class RiotGamesLeagueEntryService : BackgroundService
    {
        private readonly RiotGamesLeagueEntryServiceOptions options;
        private readonly ILogger<RiotGamesLeagueEntryService> logger;
        private readonly IRiotGamesApiService apiService;
        private readonly IRiotGamesAccountService accountService;

        private static readonly HashSet<LeagueEntryDTO> leagueEntryDTOs = [];

        public event Action<IList<LeagueEntryDTO>>? OnNewLeagueEntryData;
        public Dictionary<string, LeagueEntryDTO> LeagueEntries { get; private set; } = [];

        public RiotGamesLeagueEntryService(IOptions<RiotGamesLeagueEntryServiceOptions> options,
                                  ILogger<RiotGamesLeagueEntryService> logger,
                                  IRiotGamesApiService apiService,
                                  IRiotGamesAccountService accountService)
        {
            this.options = options.Value;
            this.logger = logger;
            this.apiService = apiService;
            this.accountService = accountService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await FetchAllLeagueEntryDataAsync();

                await Task.Delay(options.RefreshInterval, stoppingToken);
            }
        }

        private async Task FetchAllLeagueEntryDataAsync()
        {
            foreach (var keyPair in accountService.Accounts)
            {
                var accountData = await FetchLeagueEntryDataAsync(keyPair.Key, keyPair.Value.TagLine);
                if (accountData == null)
                {
                    if (logger.IsEnabled(LogLevel.Warning))
                        logger.LogWarning("No league entry data found for PUUID: {Puuid}", keyPair);
                    continue;
                }

                if (!LeagueEntries.TryAdd(keyPair.Key, accountData))
                {
                    LeagueEntries[keyPair.Key] = accountData;
                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("Updated account data for PUUID: {Puuid}", keyPair);
                }
                else
                {
                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("Added new account data for PUUID: {Puuid}", keyPair);
                }
            }
        }

        private async Task<LeagueEntryDTO?> FetchLeagueEntryDataAsync(string puuid, string tagLine)
        {
            RiotGamesRegions regions = apiService.GetRegionsFromTagLine(tagLine);

            string url = $"{apiService.GetUrl(regions)}/lol/league/v4/entries/by-summoner/{puuid}";
            return await apiService.GetAsync<LeagueEntryDTO>(url);
        }
    }
}
