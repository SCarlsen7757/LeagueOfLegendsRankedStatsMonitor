using LeagueOfLegendsInFluxTelegrafAgent.Dto.RiotGames;

namespace LeagueOfLegendsInFluxTelegrafAgent.Services.RiotGames.Interfaces
{
    public interface IAccountService
    {
        IReadOnlyDictionary<string, AccountDto> Accounts { get; }
        Task<AccountDto?> FetchAccountByRiotIdAsync(string gameName, string tagLine);
        bool AddAccount(AccountDto account);
        bool RemoveAccount(string puuId);
    }
}