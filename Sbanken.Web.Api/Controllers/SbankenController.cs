using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sbanken.Integration;

namespace Sbanken.Web.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SbankenController : ControllerBase
{
    private readonly ISbankenConnector _sbankenConnector;

    public SbankenController(ISbankenConnector sbankenConnector)
    {
        _sbankenConnector = sbankenConnector;
    }
        
    [HttpGet("Transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery]string accountName)
    {
        return string.IsNullOrEmpty(accountName) ? 
            Ok(await _sbankenConnector.GetTransactionsRaw()) : 
            Ok(await _sbankenConnector.GetTransactionsRaw(accountName));
    }
        
    [HttpGet("ArchivedTransactions")]
    public async Task<IActionResult> GetArchivedTransactions([FromQuery]string accountName)
    {
        return string.IsNullOrEmpty(accountName) ? 
            Ok(await _sbankenConnector.GetArchivedTransactionsRaw()) : 
            Ok(await _sbankenConnector.GetArchivedTransactionsRaw(accountName));
    }
        
    [HttpGet("Accounts")]
    public async Task<IActionResult> GetAccounts()
    {
        var accounts = await _sbankenConnector.GetAccountsRaw();

        return Ok(accounts);
    }
}