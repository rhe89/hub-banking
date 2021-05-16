using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hub.Storage.Repository.Core;
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
            var transactions = await _dbRepository
                .WhereAsync<Transaction, TransactionDto>(
                    x => x.Text.ToLowerInvariant().Contains(text.ToLowerInvariant()),
                    source => source
                .Include(x => x.Account));

            return transactions
                .OrderByDescending(x => x.TransactionDate)
                .ToList();
        }
        
        public async Task<IList<TransactionDto>> GetTransactionsInAccount(string accountName)
        {
            return await GetTransactionsInAccount(accountName, DateTime.Now.Month, DateTime.Now.Year);
        }

        public async Task<IList<TransactionDto>> GetTransactionsInAccount(string accountName, int? month, int? year)
        {
            month ??= DateTime.Now.Month;
            year ??= DateTime.Now.Year;
            
            var transactions = await _dbRepository
                .WhereAsync<Transaction, TransactionDto>(x => x.Account.Name == accountName && 
                                                              x.TransactionDate.Month == month  && 
                                                              x.TransactionDate.Year == year,
                    source => source.Include(x => x.Account));

            return transactions
                .OrderByDescending(x => x.TransactionDate)
                .ToList();        
        }

        public async Task<IList<TransactionDto>> GetTransactions(int ageInDays)
        {
            var transactions = await _dbRepository
                .WhereAsync<Transaction, TransactionDto>(x => x.TransactionDate > DateTime.Now.AddDays(-ageInDays),
                    source => source.Include(x => x.Account));

            return transactions
                .OrderByDescending(x => x.TransactionDate)
                .ToList();        
        }
    }
}
