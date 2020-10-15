using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Hub.Storage.Repository;
using Sbanken.Data.Entities;
using Sbanken.Dto.Api;

namespace Sbanken.Providers
{
    public class TransactionProvider : ITransactionProvider
    {
        private readonly IDbRepository _dbRepository;

        public TransactionProvider(IDbRepository dbRepository) 
        {
            _dbRepository = dbRepository;
        }
        
        public async Task<IList<TransactionDto>> GetTransactionsWithText(string text)
        {
            var transactions = await _dbRepository
                .GetManyAsync<Transaction>(
                    x => x.Text.ToLower(new CultureInfo("no")).Contains(text.ToLower()), nameof(Transaction.Account));
            
            return transactions.Select(EntityToDtoMapper.Map).ToList();
        }
        
        public async Task<IList<TransactionDto>> GetTransactionsInAccount(string accountName)
        {
            var transactions = await _dbRepository
                .GetManyAsync<Transaction>(
                    x => x.Account.Name == accountName, nameof(Transaction.Account));
            
            return transactions.Select(EntityToDtoMapper.Map).ToList();
        }

        public async Task<IList<TransactionDto>> GetTransactions(int ageInDays)
        {
            var transactions = await _dbRepository
                .GetManyAsync<Transaction>(
                    x => x.TransactionDate > DateTime.Now.AddDays(-ageInDays), nameof(Transaction.Account));
            
            return transactions.Select(EntityToDtoMapper.Map).ToList();
        }
    }
}
