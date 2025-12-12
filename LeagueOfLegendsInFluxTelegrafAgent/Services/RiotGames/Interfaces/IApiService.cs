using LeagueOfLegendsInFluxTelegrafAgent.Enums.RiotGames;

namespace LeagueOfLegendsInFluxTelegrafAgent.Services.RiotGames.Interfaces
{
    public interface IApiService
    {
        string GetUrl(Platforms region);
        string GetUrl(Regions region);
        Regions Region { get; }
        Task<T?> GetAsync<T>(string url);
    }
}