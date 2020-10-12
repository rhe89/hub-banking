using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sbanken.Providers;

namespace Sbanken.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionProvider _transactionProvider;
        private readonly ITransactionSummaryProvider _transactionSummaryProvider;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ITransactionProvider transactionProvider, ITransactionSummaryProvider transactionSummaryProvider, ILogger<TransactionController> logger)
        {
            _transactionProvider = transactionProvider;
            _transactionSummaryProvider = transactionSummaryProvider;
            _logger = logger;
        }
        
        [HttpGet("mikrospar")]
        public async Task<IActionResult> Mikrospar()
        {
            _logger.LogInformation("Request received");

            var savings = await _transactionSummaryProvider.GetMikrosparTransactions();

            return Ok(savings);
        }
        
        [HttpGet("investments")]
        public async Task<IActionResult> Investments()
        {
            _logger.LogInformation("Request received");

            var savings = await _transactionSummaryProvider.GetInvestmentTransactions();

            return Ok(savings);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> Transactions(int ageInDays)
        {
            _logger.LogInformation("Request received");

            var transactions = await _transactionProvider.GetTransactions(ageInDays);

            return Ok(transactions);
        }
    }
}
