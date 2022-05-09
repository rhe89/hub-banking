using System.Linq;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.Storage.Repository.Core;
using Hub.Shared.Storage.ServiceBus;

namespace Banking.Services;

public interface IAccountService
{
    Task<bool> AddAccount(AccountDto account);
    Task<bool> UpdateAccount(AccountDto account);
    Task<bool> UpdateAccountBalance(long accountId, decimal amount);
}

public class AccountService : IAccountService
{
    private readonly IHubDbRepository _dbRepository;
    private readonly IMessageSender _messageSender;

    public AccountService(IHubDbRepository dbRepository, IMessageSender messageSender)
    {
        _dbRepository = dbRepository;
        _messageSender = messageSender;
    }
    
    public async Task<bool> AddAccount(AccountDto account)
    {
        var accountsWithSameName =
            await _dbRepository.WhereAsync<Account, AccountDto>(x => x.Name == account.Name && x.Bank == account.Bank);

        if (accountsWithSameName.Any())
        {
            return false;
        }
        
        var addedEntity = await _dbRepository.AddAsync<Account, AccountDto>(account);
        
        _dbRepository.QueueAdd<AccountBalance, AccountBalanceDto>(new AccountBalanceDto { AccountId = addedEntity.Id, Balance = addedEntity.Balance});

        await _messageSender.AddToQueue(QueueNames.BankingAccountsUpdated);
        await _messageSender.AddToQueue(QueueNames.BankingAccountBalanceHistoryUpdated);

        return true;
    }

    public async Task<bool> UpdateAccountBalance(long accountId, decimal amount)
    {
        var accountInDb = await _dbRepository.SingleAsync<Account, AccountDto>(a => a.Id == accountId);

        var newBalance = accountInDb.Balance + amount;

        return await UpdateAccount(accountInDb, newBalance);
    }

    public async Task<bool> UpdateAccount(AccountDto account)
    {
        var accountInDb = await _dbRepository.SingleAsync<Account, AccountDto>(a => a.Id == account.Id);

        accountInDb.Name = account.Name;
        accountInDb.AccountType = account.AccountType;
        accountInDb.Bank = account.Bank;

        return await UpdateAccount(accountInDb, account.Balance);
    }
    
    private async Task<bool> UpdateAccount(AccountDto accountInDb, decimal balance)
    {
        if (accountInDb.Balance != balance)
        {
            accountInDb.Balance = balance;
            _dbRepository.QueueAdd<AccountBalance, AccountBalanceDto>(new AccountBalanceDto { AccountId = accountInDb.Id, Balance = balance});
        }
        
        _dbRepository.QueueUpdate<Account, AccountDto>(accountInDb);

        await _dbRepository.ExecuteQueueAsync();

        await _messageSender.AddToQueue(QueueNames.BankingAccountsUpdated);
        await _messageSender.AddToQueue(QueueNames.BankingAccountBalanceHistoryUpdated);

        return true;
    }
}