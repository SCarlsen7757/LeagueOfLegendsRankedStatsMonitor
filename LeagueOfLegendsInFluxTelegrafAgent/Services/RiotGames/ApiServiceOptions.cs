using LeagueOfLegendsInFluxTelegrafAgent.Enums.RiotGames;

namespace LeagueOfLegendsInFluxTelegrafAgent.Services.RiotGames
{
    public class ApiServiceOptions
    {
        public required string Token { get; set; }
        public required Regions Region { get; set; }
    }
}
