using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sbanken.Core.Providers;

namespace Sbanken.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountBalanceController : ControllerBase
    {
        private readonly IAccountBalanceProvider _accountBalanceProvider;

        public AccountBalanceController(IAccountBalanceProvider accountBalanceProvider)
        {
            _accountBalanceProvider = accountBalanceProvider;
        }
        
        public async Task<IActionResult> GetAccountBalances(string accountName, string accountType)
        {
            var accounts = await _accountBalanceProvider.GetAccountBalances(accountName, accountType);

            return Ok(accounts);
        }
    }
}
