using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sbanken.Core.Integration;

namespace Sbanken.Web.Api.Controllers
{
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
            var transactions = await _sbankenConnector.GetTransactionsRaw(accountName);

            return Ok(transactions);
        }
        
        [HttpGet("ArchivedTransactions")]
        public async Task<IActionResult> GetArchivedTransactions([FromQuery]string accountName)
        {
            var transactions = await _sbankenConnector.GetArchivedTransactionsRaw(accountName);

            return Ok(transactions);
        }
        
        [HttpGet("Accounts")]
        public async Task<IActionResult> GetAccounts()
        {
            var accounts = await _sbankenConnector.GetAccountsRaw();

            return Ok(accounts);
        }
    }
}