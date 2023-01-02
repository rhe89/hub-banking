using System;
using System.Linq;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Banking.Providers;
using Banking.Shared;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.Repository;
using Hub.Shared.Storage.ServiceBus;
using Hub.Shared.Utilities;
using Microsoft.Extensions.Logging;

namespace Banking.Services;

public interface IAccountService
{
    Task<AccountDto> Add(AccountDto newAccount);
    Task<bool> Update(AccountDto updatedAccount, bool saveChanges);

    Task<AccountDto> GetOrAdd(
        string name,
        string accountType,
        string accountNumber);

    Task Delete(AccountDto account);
    Task SaveChanges();
}

public class AccountService : IAccountService
{
    private readonly ICacheableHubDbRepository _dbRepository;
    private readonly IAccountProvider _accountProvider;
    private readonly IAccountBalanceProvider _accountBalanceProvider;
    private readonly IBankService _bankService;
    private readonly IMessageSender _messageSender;
    private readonly ILogger<AccountService> _logger;

    public AccountService(ICacheableHubDbRepository dbRepository, 
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
    
    public async Task<AccountDto> Add(AccountDto newAccount)
    {
        var accountsWithSameNumber = await _accountProvider.Get(new AccountQuery
        {
            AccountNumber = newAccount.AccountNumber
        });
        
        if (accountsWithSameNumber.Any())
        {
            return accountsWithSameNumber.First();
        }
        
        _logger.LogInformation("Creating account {Name}", newAccount.Name);

        var addedAccount = await _dbRepository.AddAsync<Account, AccountDto>(newAccount);

        if (newAccount.BalanceDate != null)
        {
            await UpdateAccountBalance(addedAccount.Id, newAccount.BalanceDate.Value, newAccount.Balance, true);
        }
        
        await NotifyConsumers();

        return addedAccount;
    }

    public async Task<bool> Update(AccountDto updatedAccount, bool saveChanges)
    {
        _logger.LogInformation("Updating account {Name} (Id: {Id})", updatedAccount.Name, updatedAccount.Id);

        var accountInDb = (await _accountProvider.Get(new AccountQuery
        {
            Id = updatedAccount.Id,
            BalanceToDate = DateTimeUtils.Today
        })).First();

        accountInDb.BankId = updatedAccount.BankId;
        accountInDb.Name = updatedAccount.Name;
        accountInDb.AccountNumber = updatedAccount.AccountNumber;
        accountInDb.AccountType = updatedAccount.AccountType;
        accountInDb.SharedAccount = updatedAccount.SharedAccount;
        accountInDb.DiscontinuedDate = updatedAccount.DiscontinuedDate;

        if (!accountInDb.BalanceIsAccumulated &&
            updatedAccount.BalanceDate != null)
        {
            await UpdateAccountBalance(accountInDb.Id, updatedAccount.BalanceDate.Value, updatedAccount.Balance, saveChanges);
        }

        _dbRepository.QueueUpdate<Account, AccountDto>(accountInDb);

        if (saveChanges)
        {
            await _dbRepository.ExecuteQueueAsync();
            await NotifyConsumers();
        }
        
        return true;
    }
    
    public async Task<AccountDto> GetOrAdd(
        string name,
        string accountType,
        string accountNumber)
    {
        var existingAccount = (await _accountProvider.Get(new AccountQuery
        {
            AccountNumber = accountNumber
        })).FirstOrDefault();

        if (existingAccount != null)
        {
            return existingAccount;
        }
        
        var bank = await _bankService.GetOrAdd("", accountNumber[..4]);
            
        var newAccount = new AccountDto
        {
            BankId = bank.Id,
            Name = name,
            AccountNumber = accountNumber,
            AccountType = accountType
        };

        return await Add(newAccount);
    }
    
    private async Task UpdateAccountBalance(long accountId, DateTime balanceDate, decimal balance, bool saveChanges)
    {
        var accountBalance = (await _accountBalanceProvider.Get(new AccountQuery
        {
            AccountId = accountId,
            BalanceFromDate = balanceDate,
            BalanceToDate = balanceDate,
        })).FirstOrDefault();

        if (accountBalance == null)
        {
            accountBalance = new AccountBalanceDto
            {
                AccountId = accountId,
                BalanceDate = balanceDate,
                Balance = balance
            };
            
            if (saveChanges)
            {
                await _dbRepository.AddAsync<AccountBalance, AccountBalanceDto>(accountBalance);
            }
            else
            {
                _dbRepository.QueueAdd<AccountBalance, AccountBalanceDto>(accountBalance);
            }
        }
        else
        {
            accountBalance.Balance = balance;
            
            if (saveChanges)
            {
                await _dbRepository.UpdateAsync<AccountBalance, AccountBalanceDto>(accountBalance);
            }
            else
            {
                _dbRepository.QueueUpdate<AccountBalance, AccountBalanceDto>(accountBalance);
            }
        }
    }

    public async Task Delete(AccountDto account)
    {
        _logger.LogInformation("Deleting account {Name} (Id: {Id})", account.Name, account.Id);

        await _dbRepository.RemoveAsync<Account, AccountDto>(account);
    }

    public async Task SaveChanges()
    {
        await _dbRepository.ExecuteQueueAsync();

        await NotifyConsumers();
    }

    private async Task NotifyConsumers()
    {
        await _messageSender.AddToQueue(QueueNames.BankingAccountsUpdated);
        await _messageSender.AddToQueue(QueueNames.CalculateCreditCardPayments);
    }
}