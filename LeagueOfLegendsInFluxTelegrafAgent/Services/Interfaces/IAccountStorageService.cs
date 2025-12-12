using LeagueOfLegendsInFluxTelegrafAgent.Dto.RiotGames;

namespace LeagueOfLegendsInFluxTelegrafAgent.Services.Interfaces
{
    public interface IAccountStorageService
    {
        Task<AccountDto?> GetAccountAsync(string puuId);
        Task<List<AccountDto>> GetAllAccountsAsync();
        Task<bool> UpsertAccountAsync(AccountDto account);
        Task<bool> DeleteAccountAsync(string puuId);
    }
}
