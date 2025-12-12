namespace LeagueOfLegendsInFluxTelegrafAgent.Services.RiotGames
{
    public class AccountServiceOptions
    {
        public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromHours(24);
    }
}
