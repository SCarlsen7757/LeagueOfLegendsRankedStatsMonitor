namespace InfluxDbDataInsert.Dto
{
    using InfluxDB.Client.Core;
    using System.Text.Json.Serialization;

    [Measurement("PlayerStats")]
    public record LeagueEntryDTO
    {
        [JsonPropertyName("leagueId")]
        public required string LeagueId { get; init; }
        [JsonPropertyName("puuid")]
        [Column("PuuId", IsTag = true)]
        public required string PuuId { get; init; }
        [JsonPropertyName("tier")]
        [JsonConverter(typeof(TierRomanJsonConverter))]
        public required LeagueTier Tier { get; init; }
        [JsonPropertyName("rank")]
        [JsonConverter(typeof(RomanIntJsonConverter))]
        public required int Rank { get; init; }
        [JsonPropertyName("leaguePoints")]
        public required int LeaguePoints { get; init; }
        [JsonPropertyName("wins")]
        [Column("Wins")]
        public required int Wins { get; init; }
        [JsonPropertyName("losses")]
        [Column("Losses")]
        public required int Losses { get; init; }
        [JsonPropertyName("hotStreak")]
        [Column("HotStreak")]
        public required bool HotStreak { get; init; }
        [JsonIgnore]
        [Column("LeaguePoints")]
        public int TotalLeaguePoints => CalculateTotalLeaguePoints();

        private int CalculateTotalLeaguePoints()
        {
            var basePoints = Tier switch
            {
                LeagueTier.Iron => 0,
                LeagueTier.Bronze => 400,
                LeagueTier.Silver => 800,
                LeagueTier.Gold => 1200,
                LeagueTier.Platinum => 1600,
                LeagueTier.Emerald => 2000,
                LeagueTier.Diamond => 2400,
                LeagueTier.Master => 2800,
                LeagueTier.Grandmaster => 3200,
                LeagueTier.Challenger => 3600,
                _ => 0,
            };
            int divisionValue = 0;
            if (Rank > 0)
            {
                divisionValue = (4 - Rank) * 100;
            }

            return basePoints + divisionValue + LeaguePoints;
        }
    }
}
