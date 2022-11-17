using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hub.Shared.Storage.Repository.Core;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;

namespace Banking.Providers;

public interface IAccountBalanceProvider
{
    Task<IList<AccountBalanceDto>> GetAccountBalances();
    Task<IList<AccountBalanceDto>> GetAccountBalances(AccountQuery accountQuery);
    Task<AccountBalanceDto> GetAccumulatedAccountBalance(AccountDto account, AccountQuery accountQuery);
}
    
public class AccountBalanceProvider : IAccountBalanceProvider
{
    private readonly IHubDbRepository _dbRepository;
    private readonly ITransactionProvider _transactionProvider;

    public AccountBalanceProvider(
        IHubDbRepository dbRepository, 
        ITransactionProvider transactionProvider)
    {
        _dbRepository = dbRepository;
        _transactionProvider = transactionProvider;
    }

    public async Task<IList<AccountBalanceDto>> GetAccountBalances()
    {
        return await GetAccountBalances(new AccountQuery());
    }

    public async Task<IList<AccountBalanceDto>> GetAccountBalances(AccountQuery accountQuery)
    {
        return await _dbRepository.GetAsync<AccountBalance, AccountBalanceDto>(GetQueryable(accountQuery));
    }
    
    public async Task<AccountBalanceDto> GetAccumulatedAccountBalance(AccountDto account, AccountQuery accountQuery)
    {
        var transactionQuery = new TransactionQuery
        {
            AccountId = account.Id,
            IncludeExcludedTransactions = true,
            IncludeTransactionsFromSharedAccounts = true
        };

        if (accountQuery.BalanceToDate != null)
        {
            transactionQuery.ToDate = accountQuery.BalanceToDate;
        }

        var transactions = await _transactionProvider.GetTransactions(transactionQuery);

        var transactionsForAccount =
            transactions.Where(x => x.AccountId == account.Id).ToList();

        if (transactionsForAccount.Count == 0)
        {
            return null;
        }

        var balance = transactionsForAccount.Sum(x => x.Amount);
        var latestTransaction = transactionsForAccount.OrderByDescending(x => x.TransactionDate).ThenByDescending(x => x.CreatedDate).First();
        
        account.BalanceIsAccumulated = true;
    
        return new AccountBalanceDto
        {
            Account = account,
            AccountId = account.Id,
            Balance = balance,
            BalanceDate = latestTransaction.TransactionDate,
            CreatedDate = latestTransaction.CreatedDate,
        };
    }

    private static Queryable<AccountBalance> GetQueryable(AccountQuery accountQuery)
    {
        return new Queryable<AccountBalance>
        {
            Query = accountQuery,
            Where = accountBalance =>
                (accountQuery.Id == accountBalance.Id) ||
                (accountQuery.AccountNumber == null || accountQuery.AccountNumber == accountBalance.Account.AccountNumber) &&
                (accountQuery.AccountType == null || accountQuery.AccountType == accountBalance.Account.AccountType) &&
                (accountQuery.AccountName == null || accountQuery.AccountName == accountBalance.Account.Name) &&
                (accountQuery.AccountId == null || accountQuery.AccountId == accountBalance.AccountId) &&
                (accountQuery.AccountIds == null || accountQuery.AccountIds.Any(accountId => accountBalance.AccountId == accountId)) &&
                (accountQuery.BankId == null || accountQuery.BankId == 0 || accountQuery.BankId == accountBalance.Account.BankId) &&
                (accountQuery.BalanceFromDate == null || accountBalance.BalanceDate >= accountQuery.BalanceFromDate.Value) &&
                (accountQuery.BalanceToDate == null || accountBalance.BalanceDate <= accountQuery.BalanceToDate.Value) &&
                (accountQuery.IncludeExternalAccounts || accountBalance.Account.Name != accountBalance.Account.AccountNumber) &&
                (accountQuery.IncludeSharedAccounts || !accountBalance.Account.SharedAccount) &&
                (accountQuery.DiscontinuedDate == null || accountQuery.IncludeDiscontinuedAccounts || accountBalance.Account.DiscontinuedDate == null || accountBalance.Account.DiscontinuedDate > accountQuery.DiscontinuedDate),
            Includes = new List<Expression<Func<AccountBalance, object>>>
            {
                accountBalance => accountBalance.Account,
                accountBalance => accountBalance.Account.Bank
            },
            OrderByDescending = accountBalance => accountBalance.BalanceDate,
            ThenOrderByDescending = accountBalance => accountBalance.CreatedDate
        };
    }

}