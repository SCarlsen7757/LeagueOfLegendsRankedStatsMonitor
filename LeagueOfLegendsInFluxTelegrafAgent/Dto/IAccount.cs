using LeagueOfLegendsInFluxTelegrafAgent.Enums.RiotGames;

namespace LeagueOfLegendsInFluxTelegrafAgent.Dto
{
    public interface IAccount
    {
        string PuuId { get; set; }
        string GameName { get; set; }
        string TagLine { get; set; }
        string PlayerName { get; }
        Platforms Platform { get; set; }
        string Team { get; set; }
    }
}