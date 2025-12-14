using LeagueOfLegendsInFluxTelegrafAgent.Enums.RiotGames;

namespace LeagueOfLegendsInFluxTelegrafAgent.Dto
{
    public class TrackedAccount : IAccount
    {
        public TrackedAccount() { }

        public TrackedAccount(RiotGames.AccountDto dto, Platforms platform, string team = "")
        {
            GameName = dto.GameName;
            TagLine = dto.TagLine;
            PuuId = dto.PuuId;
            Platform = platform;
            Team = string.IsNullOrWhiteSpace(team) ? string.Empty : team;
        }

        public string PuuId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string TagLine { get; set; } = string.Empty;
        public string PlayerName => $"{GameName}#{TagLine}";
        public Platforms Platform { get; set; }
        public string Team { get; set; } = string.Empty;
    }
}
