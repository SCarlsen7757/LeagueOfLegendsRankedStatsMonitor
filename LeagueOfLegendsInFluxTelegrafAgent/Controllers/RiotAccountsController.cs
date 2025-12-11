using InfluxDbDataInsert.Dto;
using LeagueOfLegendsInFluxTelegrafAgent.Dto;
using LeagueOfLegendsInFluxTelegrafAgent.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeagueOfLegendsInFluxTelegrafAgent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RiotAccountsController : ControllerBase
    {
        private readonly RiotGamesAccountService accountService;
        private readonly ILogger<RiotAccountsController> logger;

        public RiotAccountsController(
            RiotGamesAccountService accountService,
            ILogger<RiotAccountsController> logger)
        {
            this.accountService = accountService;
            this.logger = logger;
        }

        // GET: api/RiotAccounts
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AccountDto>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<AccountDto>> GetAll()
        {
            return Ok(accountService.Accounts.Values);
        }

        // GET api/RiotAccounts/{puuid}
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

        // POST api/RiotAccounts
        [HttpPost]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AccountDto>> Post([FromBody] AddAccountRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Fetch account from Riot API to verify it exists
                var account = await accountService.FetchAccountByRiotIdAsync(request.GameName, request.TagLine);

                if (account == null)
                {
                    if (logger.IsEnabled(LogLevel.Warning))
                        logger.LogWarning("Account not found: {GameName}#{TagLine}", request.GameName, request.TagLine);

                    return NotFound(new { message = $"Riot account '{request.GameName}#{request.TagLine}' not found" });
                }

                // Add account to the service
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

        // DELETE api/RiotAccounts/{puuid}
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
