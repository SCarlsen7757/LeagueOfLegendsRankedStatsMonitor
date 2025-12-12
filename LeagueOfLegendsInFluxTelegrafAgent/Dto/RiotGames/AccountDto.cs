using LeagueOfLegendsInFluxTelegrafAgent.Enums.RiotGames;
using System.Text.Json.Serialization;

namespace LeagueOfLegendsInFluxTelegrafAgent.Dto.RiotGames
{
    public record AccountDto
    {
        [JsonPropertyName("puuId")]
        public required string PuuId { get; init; }
        [JsonPropertyName("gameName")]
        public required string GameName { get; init; }
        [JsonPropertyName("tagLine")]
        public required string TagLine { get; init; }
        public Platforms Platform { get; set; } = Platforms.EUW1;
    }
}
