using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hub.Shared.Storage.Repository.Core;
using Banking.Data.Entities;
using Banking.Integration.Dto;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.Constants;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Banking.Services;

public interface ITransactionService
{
    Task<bool> AddTransaction(TransactionDto newTransaction, bool saveChanges);
    Task AddTransactionsFromFile(string fileName, IEnumerable<BulderBankTransaction> transactionsToImport);
    Task AddTransactionsFromFile(string fileName, IEnumerable<SbankenTransaction> transactionsToImport);
    Task<bool> UpdateTransaction(TransactionDto transaction, bool saveChanges);
    Task DeleteTransaction(TransactionDto transaction, bool saveChanges);
    Task<int> CategorizeTransactions();
    Task SaveChanges();
}

public class TransactionService : ITransactionService
{
    private readonly ITransactionProvider _transactionProvider;
    private readonly IScheduledTransactionProvider _scheduledTransactionProvider;
    private readonly IScheduledTransactionService _scheduledTransactionService;
    private readonly IAccountService _accountService;
    private readonly ITransactionCategoryService _transactionCategoryService;
    private readonly ITransactionCategoryProvider _transactionCategoryProvider;
    private readonly IHubDbRepository _dbRepository;
    private readonly IMessageSender _messageSender;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionCategoryService transactionCategoryService,
        ITransactionCategoryProvider transactionCategoryProvider,
        ITransactionProvider transactionProvider,
        IScheduledTransactionProvider scheduledTransactionProvider,
        IScheduledTransactionService scheduledTransactionService,
        IAccountService accountService,
        IHubDbRepository dbRepository,
        IMessageSender messageSender,
        ILogger<TransactionService> logger)
    {
        _transactionCategoryService = transactionCategoryService;
        _transactionCategoryProvider = transactionCategoryProvider;
        _transactionProvider = transactionProvider;
        _scheduledTransactionProvider = scheduledTransactionProvider;
        _scheduledTransactionService = scheduledTransactionService;
        _accountService = accountService;
        _dbRepository = dbRepository;
        _messageSender = messageSender;
        _logger = logger;
    }

    public async Task<bool> AddTransaction(TransactionDto newTransaction, bool saveChanges)
    {
        _logger.LogInformation(
            "Creating transaction {Description} on {Date}",
            newTransaction.Description,
            newTransaction.TransactionDate.ToString("dd.MM.yyyy"));

        _dbRepository.QueueAdd<Transaction, TransactionDto>(newTransaction);

        if (saveChanges)
        {
            await SaveChanges();
        }

        return true;
    }

    //TODO Reduce amount of calls to db
    public async Task AddTransactionsFromFile(string fileName, IEnumerable<BulderBankTransaction> transactionsToImport)
    {
        var transactionsFromNewestToOldest = transactionsToImport.OrderBy(x => x.TransactionDate).ToList();

        var dateOfOldestTransactionToImport = transactionsFromNewestToOldest.First().TransactionDate;
        var dateOfNewestTransactionToImport = transactionsFromNewestToOldest.Last().TransactionDate;

        var existingTransactionsOlderThanOldestTransactionToImport = await _transactionProvider
            .GetTransactions(new TransactionQuery
            {
                FromDate = dateOfOldestTransactionToImport,
                IncludeExcludedTransactions = true,
                IncludeTransactionsFromSharedAccounts = true
            });

        _logger.LogWarning(
            "Starting import of {Count} transactions from file {FileName} from {From} to {To}",
            transactionsFromNewestToOldest.Count,
            fileName,
            dateOfOldestTransactionToImport.ToString("dd.MM.yyyy"),
            dateOfNewestTransactionToImport.ToString("dd.MM.yyyy"));

        var iteration = 1;

        foreach (var transactionsChunck in transactionsFromNewestToOldest.Chunk(50))
        {
            _logger.LogInformation(
                "Importing {From} of {To}",
                (iteration - 1) * 50,
                iteration * transactionsChunck.Length);

            foreach (var transactionToImport in transactionsChunck)
            {
                if (
                    transactionToImport.Text == "Betaling" &&
                    transactionToImport.TransactionDate.Date >= DateTime.Now.AddDays(-3).Date)
                {
                    continue;
                }
                
                if (transactionToImport.AmountIn > 0 && !string.IsNullOrEmpty(transactionToImport.ToAccountNumber))
                {
                    await AddTransaction(
                        transactionToImport.ToAccountNumber,
                        transactionToImport.TransactionDate,
                        transactionToImport.Text,
                        transactionToImport.Category,
                        transactionToImport.SubCategory,
                        transactionToImport.AmountIn.Value,
                        fileName,
                        existingTransactionsOlderThanOldestTransactionToImport);
                }

                if (transactionToImport.AmountOut < 0 && !string.IsNullOrEmpty(transactionToImport.FromAccountNumber))
                {
                    await AddTransaction(
                        transactionToImport.FromAccountNumber,
                        transactionToImport.TransactionDate,
                        transactionToImport.Text,
                        transactionToImport.Category,
                        transactionToImport.SubCategory,
                        transactionToImport.AmountOut.Value,
                        fileName,
                        existingTransactionsOlderThanOldestTransactionToImport);
                }
            }

            await _dbRepository.ExecuteQueueAsync();
            await _scheduledTransactionService.SaveChanges();

            iteration++;
        }
    }

    public async Task AddTransactionsFromFile(string fileName, IEnumerable<SbankenTransaction> transactionsToImport)
    {
        var transactionsFromNewestToOldest = transactionsToImport.OrderBy(x => x.AccountingDate).ToList();

        var dateOfOldestTransactionToImport = transactionsFromNewestToOldest.First().AccountingDate;
        var dateOfNewestTransactionToImport = transactionsFromNewestToOldest.Last().AccountingDate;

        _logger.LogWarning(
            "Starting import of {Count} transactions from file {FileName} from {From} to {To}",
            transactionsFromNewestToOldest.Count,
            fileName,
            dateOfOldestTransactionToImport.ToString("dd.MM.yyyy"),
            dateOfNewestTransactionToImport.ToString("dd.MM.yyyy"));

        var accountNumber = fileName.Split("_")[0];

        var iteration = 1;

        foreach (var transactionsChunck in transactionsFromNewestToOldest.Chunk(50))
        {
            _logger.LogInformation(
                "Importing {From} of {To}",
                (iteration - 1) * 50,
                iteration * transactionsChunck.Length);

            foreach (var transactionToImport in transactionsChunck)
            {
                await AddTransaction(
                    accountNumber,
                    transactionToImport.AccountingDate,
                    transactionToImport.Text,
                    transactionToImport.AmountIn ?? (transactionToImport.AmountOut != null ? 
                        decimal.Negate(transactionToImport.AmountOut.Value) : 
                        0),
                    fileName);
            }

            await _dbRepository.ExecuteQueueAsync();
            await _scheduledTransactionService.SaveChanges();

            iteration++;
        }
    }
    
    private async Task AddTransaction(
        string accountNumber,
        DateTime transactionDate,
        string transactionText,
        decimal amount,
        string filename)
    {
        var account = await _accountService.GetOrAddAccount(
            accountNumber,
            AccountTypes.Standard,
            accountNumber);

        var transactionId =
            $"{account.AccountNumber}-{transactionDate}-{transactionText}-{amount}";
        
        var newTransaction = new TransactionDto
        {
            AccountId = account.Id,
            TransactionId = transactionId,
            Amount = amount,
            TransactionDate = transactionDate,
            Description = transactionText,
            Source = $"CsvImport-{filename}"
        };

        await AddTransaction(newTransaction, false);
    }
    
    private async Task AddTransaction(
        string accountNumber,
        DateTime transactionDate,
        string transactionText,
        string transactionCategory,
        string transactionSubCategory,
        decimal amount,
        string filename,
        IEnumerable<TransactionDto> existingTransactions)
    {
        var account = await _accountService.GetOrAddAccount(
            accountNumber,
            AccountTypes.Standard,
            accountNumber);

        var transactionId =
            $"{account.AccountNumber}-{transactionDate}-{transactionText}-{amount}";

        var existingTransaction = existingTransactions.FirstOrDefault(x => x.TransactionId == transactionId);

        TransactionSubCategoryDto transactionSubCategoryItem = null;

        if (!string.IsNullOrEmpty(transactionCategory))
        {
            transactionSubCategoryItem =
                await _transactionCategoryService.GetOrAddTransactionSubCategory(transactionCategory.ToLowerInvariant(),
                                                                                 transactionSubCategory.ToLowerInvariant());

            _logger.LogInformation("Got category with name {Name} and id {Id}", transactionSubCategoryItem.Name, transactionSubCategoryItem.Id);
            
            var scheduledTransaction = (await _scheduledTransactionProvider.GetScheduledTransactions(new ScheduledTransactionQuery
            {
                AmountRange = new[] { amount - 10, amount + 10 },
                NextTransactionFromDate = transactionDate.AddDays(-2),
                NextTransactionToDate = transactionDate.AddDays(2),
                TransactionSubCategoryId = transactionSubCategoryItem.Id
            })).FirstOrDefault();

            if (scheduledTransaction != null)
            {
                _logger.LogInformation(
                    "Found scheduled transaction {ScheduledTransactionDescription} scheduled on {ScheduledTransactionDate} for transaction {TransactionDescription} on {TransactionDate}",
                    scheduledTransaction.Description,
                    scheduledTransaction.NextTransactionDate,
                    transactionText,
                    transactionDate);

                await _scheduledTransactionService.SetScheduledTransactionCompleted(scheduledTransaction, true, false);
            }
        }

        if (existingTransaction != null)
        {
            existingTransaction.TransactionSubCategoryId = transactionSubCategoryItem?.Id;

            await UpdateTransaction(existingTransaction, false);
            return;
        }

        var newTransaction = new TransactionDto
        {
            AccountId = account.Id,
            TransactionId = transactionId,
            Amount = amount,
            TransactionDate = transactionDate,
            Description = transactionText,
            TransactionSubCategoryId = transactionSubCategoryItem?.Id,
            Source = $"CsvImport-{filename}"
        };

        await AddTransaction(newTransaction, false);
    }
    
    public async Task<bool> UpdateTransaction(TransactionDto transaction, bool saveChanges)
    {
        _logger.LogInformation(
            "Updating transaction {Description} on {Date} (Id: {Id})",
            transaction.Description,
            transaction.TransactionDate.ToString("dd.MM.yyyy"),
            transaction.Id);

        var transactionInDb = (await _transactionProvider.GetTransactions(new TransactionQuery
        {
            Id = transaction.Id
        })).Single();

        transactionInDb.AccountId = transaction.AccountId;
        transactionInDb.Description = transaction.Description;
        transactionInDb.TransactionSubCategoryId = transaction.TransactionSubCategoryId;
        transactionInDb.TransactionDate = transaction.TransactionDate;
        transactionInDb.Amount = transaction.Amount;
        transactionInDb.Exclude = transaction.Exclude;

        _dbRepository.QueueUpdate<Transaction, TransactionDto>(transactionInDb);

        if (saveChanges)
        {
            await SaveChanges();
        }

        return true;
    }

    public async Task DeleteTransaction(TransactionDto transaction, bool saveChanges)
    {
        _logger.LogInformation(
            "Deleting transaction {Description} on {Date} (Id: {Id})",
            transaction.Description,
            transaction.TransactionDate.ToString("dd.MM.yyyy"),
            transaction.Id);

        _dbRepository.QueueRemove<Transaction, TransactionDto>(transaction);

        if (saveChanges)
        {
            await SaveChanges();
        }
    }

    public async Task<int> CategorizeTransactions()
    {
        var uncategorizedTransactions = (await _transactionProvider.GetTransactions())
            .Where(x => x.TransactionSubCategoryId == null);

        var categorizedTransactionsCount = 0;

        var transactionSubCategories = await _transactionCategoryProvider.GetTransactionSubCategories();
        var scheduledTransactions = await _scheduledTransactionProvider.GetScheduledTransactions();

        foreach (var uncategorizedTransaction in uncategorizedTransactions)
        {
            var scheduledTransaction = scheduledTransactions
                .FirstOrDefault(scheduledTransaction => uncategorizedTransaction.Description.Contains(scheduledTransaction.Description) ||
                                                        ((scheduledTransaction.Amount == uncategorizedTransaction.Amount + 10 ||
                                                          scheduledTransaction.Amount == uncategorizedTransaction.Amount - 10) &&
                                                         (scheduledTransaction.NextTransactionDate ==
                                                          uncategorizedTransaction.TransactionDate.AddDays(3) ||
                                                          scheduledTransaction.NextTransactionDate ==
                                                          uncategorizedTransaction.TransactionDate.AddDays(-3))));

            var transactionSubCategoryId = scheduledTransaction?.TransactionSubCategoryId;

            if (scheduledTransaction != null)
            {
                scheduledTransaction.NextTransactionDate = uncategorizedTransaction.TransactionDate;
                
                await _scheduledTransactionService.UpdateScheduledTransaction(scheduledTransaction, false);
            }

            transactionSubCategoryId ??= transactionSubCategories
                .FirstOrDefault(x => x.KeywordList.Any(keyword =>
                                                           uncategorizedTransaction.Description.Contains(keyword.Value)))
                ?.Id;

            if (transactionSubCategoryId == null)
            {
                continue;
            }

            uncategorizedTransaction.TransactionSubCategoryId = transactionSubCategoryId;

            await UpdateTransaction(uncategorizedTransaction, false);

            categorizedTransactionsCount++;
        }

        await SaveChanges();

        return categorizedTransactionsCount;
    }

    public async Task SaveChanges()
    {
        await _dbRepository.ExecuteQueueAsync();

        await _messageSender.AddToQueue(QueueNames.BankingTransactionsUpdated);
    }
}