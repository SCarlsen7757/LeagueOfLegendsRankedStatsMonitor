using InfluxDbDataInsert.Dto;

namespace LeagueOfLegendsInFluxTelegrafAgent.Services
{
    public interface IRiotGamesAccountService
    {
        Dictionary<string, AccountDto> Accounts { get; }
    }
}