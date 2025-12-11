namespace LeagueOfLegendsInFluxTelegrafAgent.Services
{
    public interface IRiotGamesApiService
    {
        string GetUrl(RiotGamesPlatforms region);
        string GetUrl(RiotGamesRegions region);

        RiotGamesRegions GetRegionsFromTagLine(string tagLine);

        Task<T?> GetAsync<T>(string url);
    }
}