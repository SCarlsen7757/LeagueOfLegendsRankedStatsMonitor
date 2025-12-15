using LeagueOfLegendsInFluxTelegrafAgent.Enums.RiotGames;

namespace LeagueOfLegendsInFluxTelegrafAgent.Dto
{
    public interface IAccount
    {
        string PuuId { get; }
        string GameName { get; set; }
        string TagLine { get; set; }
        string Player { get; }
        Platforms Platform { get; }
        string Team { get; set; }
    }
}