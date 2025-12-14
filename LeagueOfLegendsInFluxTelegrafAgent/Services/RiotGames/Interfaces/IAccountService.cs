using LeagueOfLegendsInFluxTelegrafAgent.Dto;
using LeagueOfLegendsInFluxTelegrafAgent.Dto.RiotGames;

namespace LeagueOfLegendsInFluxTelegrafAgent.Services.RiotGames.Interfaces
{
    public interface IAccountService
    {
        IReadOnlyDictionary<string, IAccount> Accounts { get; }
        Task<AccountDto?> FetchAccountByPuuidAsync(string puuid);
        Task<AccountDto?> FetchAccountByNameAndTagAsync(string gameName, string tagLine);
        bool AddAccount(IAccount account);
        bool RemoveAccount(string puuId);
    }
}