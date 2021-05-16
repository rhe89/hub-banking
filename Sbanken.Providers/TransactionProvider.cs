using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public async Task<IList<TransactionDto>> GetTransactions(int? ageInDays, string description, string accountName)
        {

            Expression<Func<Transaction, bool>> predicate = transaction =>
                (ageInDays == null || transaction.TransactionDate > DateTime.Now.AddDays(-ageInDays.Value))
                && (string.IsNullOrEmpty(description) || transaction.Text.ToLower().Contains(description.ToLower()))
                && (string.IsNullOrEmpty(accountName) || transaction.Account.Name.ToLower().Contains(accountName.ToLower()));

            var transactions = await _dbRepository
                .WhereAsync<Transaction, TransactionDto>(predicate,
                    source => source.Include(x => x.Account));

            return transactions
                .OrderByDescending(x => x.TransactionDate)
                .ToList();        
        }
    }
}
