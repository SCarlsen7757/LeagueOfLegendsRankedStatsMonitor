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

        [StringLength(64, ErrorMessage = "Team cannot be longer than 64 characters")]
        public string Team { get; init => field = string.IsNullOrWhiteSpace(value) ? string.Empty : value; } = string.Empty;
    }
}
