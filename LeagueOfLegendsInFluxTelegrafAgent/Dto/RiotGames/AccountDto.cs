using LeagueOfLegendsInFluxTelegrafAgent.Enums.RiotGames;
using System.ComponentModel.DataAnnotations.Schema;
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
        [JsonIgnore]
        [NotMapped]
        public string PlayerName => $"{GameName}#{TagLine}";
        [JsonIgnore]
        public Platforms Platform { get; set; } = Platforms.EUW1;
        [JsonIgnore]
        public string Team { get; set; } = string.Empty;
    }
}
