using System;
using System.Threading.Tasks;
using Hub.Storage.Repository.Core;
using Sbanken.Core.Dto.Data;
using Sbanken.Core.Entities;
using Sbanken.Core.Providers;
using Sbanken.Core.Services;

namespace Sbanken.Services
{
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