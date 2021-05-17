using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sbanken.Core.Providers;

namespace Sbanken.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionProvider _transactionProvider;

        public TransactionsController(ITransactionProvider transactionProvider)
        {
            _transactionProvider = transactionProvider;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery]int? ageInDays, 
            [FromQuery]string description, 
            [FromQuery]string accountName)
        {
            var transactions = await _transactionProvider.GetTransactions(ageInDays, description, accountName);

            return Ok(transactions);
        }
    }
}
