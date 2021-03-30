using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sbanken.Core.Constants;
using Sbanken.Core.Providers;

namespace Sbanken.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionProvider _transactionProvider;
        private readonly ITransactionSummaryProvider _transactionSummaryProvider;

        public TransactionController(ITransactionProvider transactionProvider, ITransactionSummaryProvider transactionSummaryProvider)
        {
            _transactionProvider = transactionProvider;
            _transactionSummaryProvider = transactionSummaryProvider;
        }
        
        [HttpGet("mikrospar")]
        public async Task<IActionResult> Mikrospar()
        {
            var savings = await _transactionSummaryProvider.GetMikrosparTransactions();

            return Ok(savings);
        }
        
        [HttpGet("investments")]
        public async Task<IActionResult> Investments()
        {
            var savings = await _transactionSummaryProvider.GetInvestmentTransactions();

            return Ok(savings);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> Transactions(int ageInDays)
        {
            var transactions = await _transactionProvider.GetTransactions(ageInDays);

            return Ok(transactions);
        }
        
        [HttpGet("BillingAccountTransactions")]
        public async Task<IActionResult> GetBillingAccountTransactions()
        {
            var transactions = await _transactionProvider.GetTransactionsInAccount(AccountConstants.BillingAccountName);

            return Ok(transactions);
        }
    }
}
