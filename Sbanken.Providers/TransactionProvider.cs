using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hub.Storage.Core.Repository;
using Microsoft.EntityFrameworkCore;
using Sbanken.Core.Dto.Data;
using Sbanken.Core.Entities;
using Sbanken.Core.Providers;

namespace Sbanken.Providers
{
    public class TransactionProvider : ITransactionProvider
    {
        private readonly IHubDbRepository _dbRepository;

        public TransactionProvider(IHubDbRepository dbRepository) 
        {
            _dbRepository = dbRepository;
        }
        
        public async Task<IList<TransactionDto>> GetTransactionsWithText(string text)
        {
            var transactions = _dbRepository
                .Where<Transaction>(
                    x => x.Text.ToLowerInvariant().Contains(text.ToLowerInvariant()))
                .Include(x => x.Account);
            
            return await _dbRepository.ProjectAsync<Transaction, TransactionDto>(transactions);
        }
        
        public async Task<IList<TransactionDto>> GetTransactionsInAccount(string accountName)
        {
            var transactions = _dbRepository
                .Where<Transaction>(x => x.Account.Name == accountName)
                .Include(x => x.Account);

            return await _dbRepository.ProjectAsync<Transaction, TransactionDto>(transactions);
        }

        public async Task<IList<TransactionDto>> GetTransactions(int ageInDays)
        {
            var transactions = _dbRepository
                .Where<Transaction>(x => x.TransactionDate > DateTime.Now.AddDays(-ageInDays))
                .Include(x => x.Account);

            return await _dbRepository.ProjectAsync<Transaction, TransactionDto>(transactions);
        }
    }
}
