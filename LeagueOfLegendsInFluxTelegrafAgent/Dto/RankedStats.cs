using LeagueOfLegendsInFluxTelegrafAgent.Dto.RiotGames;
using LeagueOfLegendsInFluxTelegrafAgent.Enums.RiotGames;

namespace LeagueOfLegendsInFluxTelegrafAgent.Dto
{
    public class RankedStats : IRankedStats
    {
        private IAccount Account { get; set; }
        public string QueueType { get; private set; }
        public LeagueTier Tier { get; private set; }
        public int Rank { get; private set; }
        public int LeaguePoints { get; private set; }
        public int Wins { get; private set; }
        public int Losses { get; private set; }
        public int TotalGames { get; private set; }
        public double WinRate { get; private set; }
        public bool HotStreak { get; private set; }
        public string PuuId => Account.PuuId;
        public string GameName { get => Account.GameName; set => Account.GameName = value; }
        public string TagLine { get => Account.TagLine; set => Account.TagLine = value; }
        public string Player => Account.Player;
        public Platforms Platform => Account.Platform;
        public string Team { get => Account.Team; set => Account.Team = value; }

        public RankedStats(IAccount account, LeagueEntryDTO leagueEntryDTO)
        {
            Account = account;
            QueueType = leagueEntryDTO.QueueType;
            Tier = leagueEntryDTO.Tier;
            Rank = leagueEntryDTO.Rank;
            LeaguePoints = leagueEntryDTO.LeaguePoints;
            Wins = leagueEntryDTO.Wins;
            Losses = leagueEntryDTO.Losses;
            TotalGames = leagueEntryDTO.TotalGames;
            WinRate = leagueEntryDTO.WinRate;
            HotStreak = leagueEntryDTO.HotStreak;
        }
    }
}
