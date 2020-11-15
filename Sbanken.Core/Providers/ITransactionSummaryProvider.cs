using System.Threading.Tasks;
using Sbanken.Core.Dto.Api;

namespace Sbanken.Core.Providers
{
    public interface ITransactionSummaryProvider
    {
        Task<TransactionSummaryDto> GetMikrosparTransactions();
        Task<TransactionSummaryDto> GetInvestmentTransactions();
    }
}