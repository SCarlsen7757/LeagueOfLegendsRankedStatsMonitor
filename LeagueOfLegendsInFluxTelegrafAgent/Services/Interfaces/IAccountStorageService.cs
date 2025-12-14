using LeagueOfLegendsInFluxTelegrafAgent.Dto;

namespace LeagueOfLegendsInFluxTelegrafAgent.Services.Interfaces
{
    public interface IAccountStorageService
    {
        Task<IAccount?> GetAccountAsync(string puuId);
        Task<IList<IAccount>> GetAllAccountsAsync();
        Task<bool> UpsertAccountAsync(IAccount account);
        Task<bool> DeleteAccountAsync(string puuId);
    }
}
