namespace LeagueOfLegendsInFluxTelegrafAgent.Dto.RiotGames
{
    using LeagueOfLegendsInFluxTelegrafAgent.Enums.RiotGames;
    using System.Text.Json.Serialization;

    public record LeagueEntryDTO
    {
        [JsonPropertyName("leagueId")]
        public required string LeagueId { get; init; }
        [JsonPropertyName("puuid")]
        public required string PuuId { get; init; }
        [JsonPropertyName("queueType")]
        public required string QueueType { get; init; }
        [JsonPropertyName("tier")]
        [JsonConverter(typeof(TierJsonConverter))]
        public required LeagueTier Tier { get; init; }
        [JsonPropertyName("rank")]
        [JsonConverter(typeof(RomanIntJsonConverter))]
        public required int Rank { get; init; }
        [JsonPropertyName("leaguePoints")]
        public required int LeaguePoints { get; init; }
        [JsonPropertyName("wins")]
        public required int Wins { get; init; }
        [JsonPropertyName("losses")]
        public required int Losses { get; init; }
        [JsonIgnore]
        public int TotalGames => Wins + Losses;
        [JsonIgnore]
        public double WinRate => TotalGames == 0 ? 0 : (double)Wins / TotalGames * 100;
        [JsonPropertyName("hotStreak")]
        public required bool HotStreak { get; init; }
        [JsonIgnore]
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
