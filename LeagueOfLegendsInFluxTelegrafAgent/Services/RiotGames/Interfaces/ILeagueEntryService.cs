using LeagueOfLegendsInFluxTelegrafAgent.Dto.RiotGames;
using LeagueOfLegendsInFluxTelegrafAgent.Enums.RiotGames;

namespace LeagueOfLegendsInFluxTelegrafAgent.Services.RiotGames.Interfaces
{
    public interface ILeagueEntryService
    {
        event Action<string, LeagueEntryDTO>? OnNewLeagueEntryData;
        Task FetchAllLeagueEntryDataAsync();
        Task<IList<LeagueEntryDTO>?> FetchLeagueEntryDataAsync(string puuid, Platforms platforms);
    }
}