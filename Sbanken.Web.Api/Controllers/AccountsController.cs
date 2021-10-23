using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Sbanken.Core.Providers;

namespace Sbanken.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountProvider _accountProvider;
        private readonly ITransactionProvider _transactionProvider;
        private readonly IAccountBalanceProvider _accountBalanceProvider;

        public AccountsController(IAccountProvider accountProvider,
            ITransactionProvider transactionProvider,
            IAccountBalanceProvider accountBalanceProvider)
        {
            _accountProvider = accountProvider;
            _transactionProvider = transactionProvider;
            _accountBalanceProvider = accountBalanceProvider;
        }
        
        public async Task<IActionResult> GetAccounts([FromQuery]string accountName, [FromQuery]string accountType)
        {
            var accounts = await _accountProvider.GetAccounts(accountName, accountType);

            var billingAccount = accounts.FirstOrDefault(x => x.Name == "Regningsbetaling");

            if (billingAccount == null)
                return Ok(accounts);
            
            var billingAccountTransactions = await _transactionProvider.GetTransactions(30, null, "Regningsbetaling");

            var lastBillingAccountTransaction = billingAccountTransactions.FirstOrDefault();

            if (lastBillingAccountTransaction == null)
                return Ok(accounts);

            var billingAccountBalances = await _accountBalanceProvider.GetAccountBalances("Regningsbetaling", null,
                lastBillingAccountTransaction.CreatedDate, null);
            
            if (billingAccountBalances.Count == 0)
                billingAccountBalances = await _accountBalanceProvider.GetAccountBalances("Regningsbetaling", null,
                    lastBillingAccountTransaction.TransactionDate, null);

            if (billingAccountBalances.Count == 0)
                return Ok(accounts);
            
            billingAccount.Balance = billingAccountBalances.FirstOrDefault() != null
                ? billingAccountBalances.First().Balance
                : billingAccount.Balance;

            return Ok(accounts);
        }
    }
}
