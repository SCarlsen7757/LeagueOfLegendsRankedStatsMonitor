using InfluxDB.Client.Core;
using System.Text.Json.Serialization;

namespace InfluxDbDataInsert.Dto
{
    [Measurement("PlayerInformation")]
    public record AccountDto
    {
        [JsonPropertyName("puuId")]
        [Column("PuuId", IsTag = true)]
        public required string PuuId { get; init; }
        [JsonPropertyName("gameName")]
        [Column("GameName")]
        public required string GameName { get; init; }
        [JsonPropertyName("tagLine")]
        [Column("TagLine")]
        public required string TagLine { get; init; }
    }
}
