using System;
using System.Linq;
using Sbanken.Data.Entities;
using Sbanken.Dto.Api;
using ValueType = Sbanken.Dto.Sbanken;

namespace Sbanken.Providers
{
    public static class EntityToDtoMapper
    {
        public static AccountDto Map(Account account)
        {
            return new AccountDto
            {
                Name = account.Name,
                Balance = account.CurrentBalance,
                AccountType = account.AccountType,
                Transactions = account.Transactions
                    .Where(x => x.TransactionDate > DateTime.Now.AddDays(-30))
                    .Select(Map)
                    .OrderByDescending(x => x.CreatedDate)
                    .ToList()
            };
        }
        
        public static TransactionDto Map(Transaction transaction)
        {
            return new TransactionDto
            {
                Name = transaction.Text,
                Amount = transaction.Amount,
                CreatedDate = transaction.TransactionDate,
                AccountName = transaction.Account.Name,
                TransactionType = transaction.TransactionType
            };
        }
    }
}