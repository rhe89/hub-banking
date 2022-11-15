using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.Query;

namespace Banking.Web.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountBalanceController : ControllerBase
{
    private readonly IAccountBalanceProvider _accountBalanceProvider;

    public AccountBalanceController(IAccountBalanceProvider accountBalanceProvider)
    {
        _accountBalanceProvider = accountBalanceProvider;
    }

    [HttpPost("history")]
    public async Task<IActionResult> GetAccountBalances([FromQuery]AccountQuery accountQuery)
    {
        var accounts = await _accountBalanceProvider.GetAccountBalances(accountQuery);
            
        return Ok(accounts);
    }
}