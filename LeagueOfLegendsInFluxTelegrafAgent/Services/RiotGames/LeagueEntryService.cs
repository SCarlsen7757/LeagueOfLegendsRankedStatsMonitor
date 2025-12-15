using LeagueOfLegendsInFluxTelegrafAgent.Dto;
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

        public event Action<IRankedStats>? OnNewLeagueEntryData;

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
                var leagueData = await FetchLeagueEntryDataAsync(keyPair.Key, keyPair.Value.Platform);
                if (leagueData == null)
                {
                    if (logger.IsEnabled(LogLevel.Warning))
                        logger.LogWarning("No league entry data found for PUUID: {Puuid}", keyPair.Key);
                    continue;
                }

                var player = keyPair.Value;
                foreach (var entry in leagueData)
                {
                    OnNewLeagueEntryData?.Invoke(new RankedStats(player, entry));
                }
            }
        }

        public async Task<IList<LeagueEntryDTO>?> FetchLeagueEntryDataAsync(string puuid, Platforms platforms)
        {
            string url = $"{apiService.GetUrl(platforms)}/lol/league/v4/entries/by-puuid/{puuid}";
            return await apiService.GetAsync<List<LeagueEntryDTO>>(url);
        }
    }
}
