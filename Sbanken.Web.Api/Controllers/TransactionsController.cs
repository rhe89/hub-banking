using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sbanken.Core.Constants;
using Sbanken.Core.Providers;

namespace Sbanken.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionProvider _transactionProvider;
        private readonly ITransactionSummaryProvider _transactionSummaryProvider;

        public TransactionsController(ITransactionProvider transactionProvider, ITransactionSummaryProvider transactionSummaryProvider)
        {
            _transactionProvider = transactionProvider;
            _transactionSummaryProvider = transactionSummaryProvider;
        }
        
        [HttpGet]
        public async Task<IActionResult> Transactions(int ageInDays)
        {
            var transactions = await _transactionProvider.GetTransactions(ageInDays);

            return Ok(transactions);
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

        [HttpGet("BillingAccountTransactions")]
        public async Task<IActionResult> GetBillingAccountTransactions(int? month, int? year)
        {
            var transactions = await _transactionProvider.GetTransactionsInAccount(AccountConstants.BillingAccountName, month, year);

            return Ok(transactions);
        }
    }
}
