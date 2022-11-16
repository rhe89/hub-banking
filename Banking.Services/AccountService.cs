using System;
using System.Linq;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.Repository;
using Hub.Shared.Storage.Repository.Core;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Banking.Services;

public interface IAccountService
{
    Task<AccountDto> AddAccount(AccountDto newAccount, bool saveChanges);
    Task<bool> UpdateAccount(AccountDto updatedAccount, bool saveChanges);
    Task<AccountDto> GetOrAddAccount(
        string name,
        string accountType,
        string accountNumber);
    Task<bool> UpdateAccountBalance(long accountId, DateTime balanceDate, decimal amount, bool saveChanges);
    Task DeleteAccount(AccountDto account, bool saveChanges);
}

public class AccountService : IAccountService
{
    private readonly IHubDbRepository _dbRepository;
    private readonly IAccountProvider _accountProvider;
    private readonly IAccountBalanceProvider _accountBalanceProvider;
    private readonly IBankService _bankService;
    private readonly IMessageSender _messageSender;
    private readonly ILogger<AccountService> _logger;

    public AccountService(IHubDbRepository dbRepository, 
                          IAccountProvider accountProvider,
                          IAccountBalanceProvider accountBalanceProvider,
                          IBankService bankService,
                          IMessageSender messageSender,
                          ILogger<AccountService> logger)
    {
        _dbRepository = dbRepository;
        _accountProvider = accountProvider;
        _accountBalanceProvider = accountBalanceProvider;
        _bankService = bankService;
        _messageSender = messageSender;
        _logger = logger;
    }
    
    public async Task<AccountDto> AddAccount(AccountDto newAccount, bool saveChanges)
    {
        var accountsWithSameName = await _accountProvider.GetAccounts(new AccountQuery
        {
            AccountName = newAccount.Name,
            BankId = newAccount.BankId
        });
        
        if (accountsWithSameName.Any())
        {
            return accountsWithSameName.First();
        }
        
        _logger.LogInformation("Creating account {Name}", newAccount.Name);

        var addedAccount = await _dbRepository.AddAsync<Account, AccountDto>(newAccount);
        
        return addedAccount;
    }

    public async Task<bool> UpdateAccount(AccountDto updatedAccount, bool saveChanges)
    {
        _logger.LogInformation("Updating account {Name} (Id: {Id})", updatedAccount.Name, updatedAccount.Id);

        var accountInDb =  (await _accountProvider.GetAccounts(new AccountQuery
        { 
            AccountId = updatedAccount.Id,
            IncludeDiscontinuedAccounts = true,
            IncludeExternalAccounts = true,
            IncludeSharedAccounts = true
        })).First();        
        
        accountInDb.BankId = updatedAccount.BankId;
        accountInDb.Name = updatedAccount.Name;
        accountInDb.AccountNumber = updatedAccount.AccountNumber;
        accountInDb.AccountType = updatedAccount.AccountType;
        accountInDb.SharedAccount = updatedAccount.SharedAccount;
        accountInDb.DiscontinuedDate = updatedAccount.DiscontinuedDate;

        await _dbRepository.UpdateAsync<Account, AccountDto>(accountInDb);

        return true;
    }
    
    public async Task<AccountDto> GetOrAddAccount(
        string name,
        string accountType,
        string accountNumber)
    {
        var existingAccount = (await _accountProvider.GetAccounts(new AccountQuery
        {
            AccountNumber = accountNumber,
            IncludeExternalAccounts = true,
        })).FirstOrDefault();

        if (existingAccount != null)
        {
            return existingAccount;
        }
        
        var bank = await _bankService.GetOrCreateBank("", accountNumber[..4]);
            
        var newAccount = new AccountDto
        {
            BankId = bank.Id,
            Name = name,
            AccountNumber = accountNumber,
            AccountType = accountType
        };

        return await AddAccount(newAccount, true);
    }
    
    public async Task<bool> UpdateAccountBalance(long accountId, DateTime balanceDate, decimal amount, bool saveChanges)
    {
        var currentAccountBalance = (await _accountBalanceProvider.GetAccountBalances(new AccountQuery
        {
            AccountId = accountId,
            BalanceToDate = balanceDate,
            IncludeExternalAccounts = true
        })).FirstOrDefault();

        var newAccountBalance = new AccountBalanceDto
        {
            AccountId = accountId,
            BalanceDate = balanceDate,
            Balance = (currentAccountBalance?.Balance ?? 0) + amount
        };
        
        await _dbRepository.AddAsync<AccountBalance, AccountBalanceDto>(newAccountBalance);
        
        return true;
    }

    public async Task DeleteAccount(AccountDto account, bool saveChanges)
    {
        _logger.LogInformation("Deleting account {Name} (Id: {Id})", account.Name, account.Id);

        _dbRepository.QueueRemove<Account, AccountDto>(account);
        
        if (saveChanges)
        {
            await _dbRepository.ExecuteQueueAsync();
        }
    }

    private async Task SaveChanges()
    {
        await _dbRepository.ExecuteQueueAsync();

        await _messageSender.AddToQueue(QueueNames.BankingAccountsUpdated);
        await _messageSender.AddToQueue(QueueNames.BankingAccountBalanceHistoryUpdated);
    }
}