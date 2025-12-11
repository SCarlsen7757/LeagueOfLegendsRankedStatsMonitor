using System.ComponentModel.DataAnnotations;

namespace LeagueOfLegendsInFluxTelegrafAgent.Dto
{
    public record AddAccountRequest
    {
        [Required]
        public required string GameName { get; init; }

        [Required]
        public required string TagLine { get; init; }
    }
}
