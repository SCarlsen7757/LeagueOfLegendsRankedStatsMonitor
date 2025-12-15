using LeagueOfLegendsInFluxTelegrafAgent.Enums.RiotGames;

namespace LeagueOfLegendsInFluxTelegrafAgent.Dto
{
    public interface IRankedStats : IAccount
    {
        public string QueueType { get; }
        public LeagueTier Tier { get; }
        public int Rank { get; }
        public int LeaguePoints { get; }
        public int Wins { get; }
        public int Losses { get; }
        public int TotalGames { get; }
        public double WinRate { get; }
        public bool HotStreak { get; }
    }
}
