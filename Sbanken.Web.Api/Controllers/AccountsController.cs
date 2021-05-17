using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sbanken.Core.Providers;

namespace Sbanken.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountProvider _accountProvider;

        public AccountsController(IAccountProvider accountProvider)
        {
            _accountProvider = accountProvider;
        }
        
        public async Task<IActionResult> GetAccounts([FromQuery]string accountName, [FromQuery]string accountType)
        {
            var accounts = await _accountProvider.GetAccounts(accountName, accountType);

            return Ok(accounts);
        }
    }
}
