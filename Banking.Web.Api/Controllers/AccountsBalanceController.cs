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
    public async Task<IActionResult> Get([FromQuery]AccountQuery accountQuery)
    {
        var accounts = await _accountBalanceProvider.Get(accountQuery);
            
        return Ok(accounts);
    }
}