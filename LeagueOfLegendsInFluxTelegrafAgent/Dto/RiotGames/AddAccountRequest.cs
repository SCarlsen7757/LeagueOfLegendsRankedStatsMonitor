using LeagueOfLegendsInFluxTelegrafAgent.Enums.RiotGames;
using System.ComponentModel.DataAnnotations;

namespace LeagueOfLegendsInFluxTelegrafAgent.Dto.RiotGames
{
    public record AddAccountRequest
    {
        [Required]
        public required string GameName { get; init; }

        [Required]
        public required string TagLine { get; init; }

        [Required]
        public required Platforms Platform { get; init; }

        public string? Team { get; init; }
    }
}
