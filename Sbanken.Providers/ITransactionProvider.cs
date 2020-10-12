using System.Collections.Generic;
using System.Threading.Tasks;
using Sbanken.Dto.Api;

namespace Sbanken.Providers
{
    public interface ITransactionProvider
    {
        Task<IList<TransactionDto>> GetTransactions(int ageInDays);
        Task<IList<TransactionDto>> GetTransactionsWithText(string text);
        Task<IList<TransactionDto>> GetTransactionsInAccount(string accountName);
    }
}