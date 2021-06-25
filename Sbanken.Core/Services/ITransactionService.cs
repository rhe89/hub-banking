using System.Threading.Tasks;

namespace Sbanken.Core.Services
{
    public interface ITransactionService
    {
        Task<bool> UpdateTransactionDescription(long transactionId, string description);
    }
}