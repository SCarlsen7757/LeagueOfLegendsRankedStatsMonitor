using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LeagueOfLegendsInFluxTelegrafAgent.Services
{
    public class RiotGamesApiServiceOptions
    {
        public required string Token { get; set; }
    }

    public class RiotGamesApiService : IRiotGamesApiService
    {
        private readonly RiotGamesApiServiceOptions options;
        private readonly ILogger<RiotGamesApiService> logger;

        public RiotGamesApiService(IOptions<RiotGamesApiServiceOptions> options, ILogger<RiotGamesApiService> logger)
        {
            this.options = options.Value;
            this.logger = logger;
        }

        private static string BaseUrl => "api.riotgames.com";

        public string GetUrl(RiotGamesPlatforms region) => $"https://{region.ToString().ToLower()}.{BaseUrl}";
        public string GetUrl(RiotGamesRegions region) => $"https://{region.ToString().ToLower()}.{BaseUrl}";

        public RiotGamesRegions GetRegionsFromTagLine(string tagLine)
        {
            return tagLine.ToUpper() switch
            {
                "NA1" or "BR1" or "LA1" or "LA2" => RiotGamesRegions.AMERICAS,
                "KR" or "JP1" => RiotGamesRegions.ASIA,
                "EUN1" or "EUW1" or "TR1" or "RU" => RiotGamesRegions.EUROPE,
                "OC1" or "PH2" or "SG2" or "TH2" or "TW2" or "VN2" => RiotGamesRegions.SEA,
                _ => throw new ArgumentOutOfRangeException(nameof(tagLine), $"Unsupported tag line: {tagLine}"),
            };
        }

        private readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<T?> GetAsync<T>(string url)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Riot-Token", options.Token);

            using var response = await client.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(content))
                return default;

            return JsonSerializer.Deserialize<T>(content, jsonSerializerOptions);
        }
    }
}
