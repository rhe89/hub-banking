using System.Collections.Generic;
using System.Threading.Tasks;
using Sbanken.Core.Dto.Data;

namespace Sbanken.Core.Providers
{
    public interface ITransactionProvider
    {
        Task<IList<TransactionDto>> GetTransactions(int? ageInDays, string description, string accountName);
        Task<TransactionDto> GetTransaction(long transactionId);
    }
}