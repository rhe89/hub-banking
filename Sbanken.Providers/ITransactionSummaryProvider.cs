using System.Threading.Tasks;
using Sbanken.Dto.Api;

namespace Sbanken.Providers
{
    public interface ITransactionSummaryProvider
    {
        Task<TransactionSummaryDto> GetMikrosparTransactions();
        Task<TransactionSummaryDto> GetInvestmentTransactions();
    }
}