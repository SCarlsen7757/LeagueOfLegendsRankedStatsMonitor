using LeagueOfLegendsInFluxTelegrafAgent.Dto.RiotGames;
using LeagueOfLegendsInFluxTelegrafAgent.Enums.RiotGames;

namespace LeagueOfLegendsInFluxTelegrafAgent.Services.RiotGames.Interfaces
{
    public interface ILeagueEntryService
    {
        Dictionary<string, LeagueEntryDTO> LeagueEntries { get; }
        event Action<IList<LeagueEntryDTO>>? OnNewLeagueEntryData;
        Task FetchAllLeagueEntryDataAsync();
        Task<LeagueEntryDTO?> FetchLeagueEntryDataAsync(string puuid, Platforms platforms);
    }
}