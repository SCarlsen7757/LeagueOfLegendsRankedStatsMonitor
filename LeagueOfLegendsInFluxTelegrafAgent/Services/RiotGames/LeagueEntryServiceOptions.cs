namespace LeagueOfLegendsInFluxTelegrafAgent.Services.RiotGames
{
    public class LeagueEntryServiceOptions
    {
        public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(30);
    }
}
