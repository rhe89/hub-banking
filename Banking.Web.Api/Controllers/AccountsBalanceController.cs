using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.SearchParameters;

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
        
    [HttpPost]
    public async Task<IActionResult> GetAccountBalances([FromQuery]AccountBalanceSearchParameters accountBalanceSearchParameters)
    {
        var accounts = await _accountBalanceProvider.GetAccountBalances(accountBalanceSearchParameters);
            
        return Ok(accounts);
    }
}