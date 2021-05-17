using System;
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
        
        [HttpGet]
        public async Task<IActionResult> GetAccountBalances([FromQuery]string accountName, 
            [FromQuery]string accountType, 
            [FromQuery]DateTime? fromDate, 
            [FromQuery]DateTime? toDate)
        {
            var accounts = await _accountBalanceProvider.GetAccountBalances(accountName, accountType, fromDate, toDate);

            return Ok(accounts);
        }
    }
}
