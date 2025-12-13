using LeagueOfLegendsInFluxTelegrafAgent.Dto.RiotGames;
using LeagueOfLegendsInFluxTelegrafAgent.Services.RiotGames.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LeagueOfLegendsInFluxTelegrafAgent.Controllers.RiotGames
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly ILogger<AccountsController> logger;
        private readonly IAccountService accountService;

        public AccountsController(
            ILogger<AccountsController> logger,
            IAccountService accountService
            )
        {
            this.logger = logger;
            this.accountService = accountService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AccountDto>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<AccountDto>> GetAll()
        {
            return Ok(accountService.Accounts.Values.ToList());
        }

        [HttpGet("{puuid}")]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<AccountDto> Get(string puuid)
        {
            if (accountService.Accounts.TryGetValue(puuid, out var account))
            {
                return Ok(account);
            }

            return NotFound(new { message = $"Account with PUUID '{puuid}' not found" });
        }

        [HttpPost]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<AccountDto>> Post([FromBody] AddAccountRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var account = await accountService.FetchAccountByRiotIdAsync(request.GameName, request.TagLine);

                if (account == null)
                {
                    if (logger.IsEnabled(LogLevel.Warning))
                        logger.LogWarning("Account not found: {GameName}#{TagLine}", request.GameName, request.TagLine);

                    return NotFound(new { message = $"Riot account '{request.GameName}#{request.TagLine}' not found" });
                }

                account = new AccountDto()
                {
                    GameName = account.GameName,
                    TagLine = account.TagLine,
                    PuuId = account.PuuId,
                    Platform = request.Platform
                };

                if (!accountService.AddAccount(account))
                {
                    return Conflict(new { message = $"Account '{request.GameName}#{request.TagLine}' is already being tracked" });
                }

                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("Successfully added account: {GameName}#{TagLine} (PUUID: {PuuId})",
                        account.GameName, account.TagLine, account.PuuId);

                return CreatedAtAction(nameof(Get), new { puuid = account.PuuId }, account);
            }
            catch (HttpRequestException ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(ex, "Error fetching account from Riot API: {GameName}#{TagLine}",
                        request.GameName, request.TagLine);

                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound(new { message = $"Riot account '{request.GameName}#{request.TagLine}' not found" });
                }

                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { message = "Unable to connect to Riot API" });
            }
            catch (Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(ex, "Unexpected error adding account: {GameName}#{TagLine}",
                        request.GameName, request.TagLine);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while processing your request" });
            }
        }

        [HttpDelete("{puuid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Delete(string puuid)
        {
            if (accountService.RemoveAccount(puuid))
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("Successfully removed account with PUUID: {PuuId}", puuid);

                return NoContent();
            }

            return NotFound(new { message = $"Account with PUUID '{puuid}' not found" });
        }
    }
}
