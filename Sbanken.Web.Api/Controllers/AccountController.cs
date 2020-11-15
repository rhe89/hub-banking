using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sbanken.Core.Providers;

namespace Sbanken.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountProvider _accountProvider;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountProvider accountProvider, ILogger<AccountController> logger)
        {
            _accountProvider = accountProvider;
            _logger = logger;
        }
        
        [HttpGet("accounts")]
        public async Task<IActionResult> Accounts()
        {
            _logger.LogInformation("Request received");

            var accounts = await _accountProvider.GetAccounts();

            return Ok(accounts);
        }

        [HttpGet("standard")]
        public async Task<IActionResult> StandardAccounts()
        {
            _logger.LogInformation("Request received");

            var accounts = await _accountProvider.GetStandardAccounts();

            return Ok(accounts);
        }
        
        [HttpGet("credit")]
        public async Task<IActionResult> CreditAccounts()
        {
            _logger.LogInformation("Request received");

            var accounts = await _accountProvider.GetCreditAccounts();

            return Ok(accounts);
        }
        
        [HttpGet("savings")]
        public async Task<IActionResult> SavingsAccounts()
        {
            _logger.LogInformation("Request received");

            var accounts = await _accountProvider.GetSavingsAccounts();

            return Ok(accounts);
        }
    }
}
