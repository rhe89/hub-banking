using System.Threading.Tasks;
using Hub.Shared.DataContracts.Sbanken;
using Hub.Shared.Storage.Repository.Core;
using Sbanken.Data.Entities;
using Sbanken.Providers;

namespace Sbanken.Services
{
    public interface ITransactionService
    {
        Task<bool> UpdateTransactionDescription(long transactionId, string description);
    }
    
    public class TransactionService : ITransactionService
    {
        private readonly IHubDbRepository _hubDbRepository;
        private readonly ITransactionProvider _transactionProvider;

        public TransactionService(IHubDbRepository hubDbRepository, ITransactionProvider transactionProvider)
        {
            _hubDbRepository = hubDbRepository;
            _transactionProvider = transactionProvider;
        }
        
        public async Task<bool> UpdateTransactionDescription(long transactionId, string description)
        {
            var transaction = await _transactionProvider.GetTransaction(transactionId);

            transaction.Description = description;

            await _hubDbRepository.UpdateAsync<Transaction, TransactionDto>(transaction);

            return true;
        }
    }
}