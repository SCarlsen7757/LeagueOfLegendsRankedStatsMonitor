using LeagueOfLegendsInFluxTelegrafAgent.Enums.RiotGames;
using LeagueOfLegendsInFluxTelegrafAgent.Services.RiotGames.Interfaces;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LeagueOfLegendsInFluxTelegrafAgent.Services.RiotGames
{
    public class ApiService : IApiService
    {
        private readonly ApiServiceOptions options;
        private readonly ILogger<ApiService> logger;

        public ApiService(IOptions<ApiServiceOptions> options, ILogger<ApiService> logger)
        {
            this.options = options.Value;
            this.logger = logger;
        }

        private static string BaseUrl => "api.riotgames.com";

        public string GetUrl(Platforms region) => $"https://{region.ToString().ToLower()}.{BaseUrl}";
        public string GetUrl(Regions region) => $"https://{region.ToString().ToLower()}.{BaseUrl}";

        public Regions Region => options.Region;

        private readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<T?> GetAsync<T>(string url)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Riot-Token", options.Token);

            using var response = await client.GetAsync(url).ConfigureAwait(false);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                    logger.LogWarning(ex, "Exception when HTTP GET URL:{url}", url);
                return default;
            }
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(content))
                return default;

            return JsonSerializer.Deserialize<T>(content, jsonSerializerOptions);
        }
    }
}
