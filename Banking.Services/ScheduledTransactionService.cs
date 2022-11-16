using System;
using System.Linq;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.Repository.Core;
using Microsoft.Extensions.Logging;

namespace Banking.Services;

public interface IScheduledTransactionService
{
    Task<ScheduledTransactionDto> AddScheduledTransaction(ScheduledTransactionDto newScheduledTransaction, bool saveChanges);
    Task SetScheduledTransactionCompleted(ScheduledTransactionDto updatedScheduledTransaction, bool completed, bool saveChanges);
    Task UpdateScheduledTransaction(ScheduledTransactionDto updatedScheduledTransaction, bool saveChanges);
    Task DeleteScheduledTransaction(ScheduledTransactionDto scheduledTransaction, bool saveChanges);
    Task SaveChanges();
}

public class ScheduledTransactionService : IScheduledTransactionService
{
    private readonly IScheduledTransactionProvider _scheduledTransactionProvider;
    private readonly IHubDbRepository _dbRepository;
    private readonly ILogger<ScheduledTransactionService> _logger;

    public ScheduledTransactionService(
        IScheduledTransactionProvider scheduledTransactionProvider,
        IHubDbRepository dbRepository,
        ILogger<ScheduledTransactionService> logger)
    {
        _scheduledTransactionProvider = scheduledTransactionProvider;
        _dbRepository = dbRepository;
        _logger = logger;
    }

    public async Task<ScheduledTransactionDto> AddScheduledTransaction(ScheduledTransactionDto newScheduledTransaction, bool saveChanges)
    {
        _logger.LogInformation("Creating scheduled transaction {Description}", newScheduledTransaction.Description);
        
        newScheduledTransaction.TransactionKey = Guid.NewGuid();
        
        if (saveChanges)
        {
            newScheduledTransaction = await _dbRepository.AddAsync<ScheduledTransaction, ScheduledTransactionDto>(newScheduledTransaction);
        }
        else
        {
            _dbRepository.QueueAdd<ScheduledTransaction, ScheduledTransactionDto>(newScheduledTransaction);
        }

        await FillScheduledTransactionForPeriod(newScheduledTransaction, DateTime.Now.AddYears(3), saveChanges);

        return newScheduledTransaction;
    }

    private async Task FillScheduledTransactionForPeriod(ScheduledTransactionDto originalScheduledTransaction, DateTime periodEnd, bool saveChanges)
    {
        if (originalScheduledTransaction.Occurrence == Occurrence.Once)
        {
            return;
        }

        var nextTransactionDate = GetNextTransactionDate(originalScheduledTransaction.NextTransactionDate, originalScheduledTransaction.Occurrence);

        while (nextTransactionDate < periodEnd)
        {
            var newScheduledTransaction = new ScheduledTransactionDto
            {
                TransactionSubCategoryId = originalScheduledTransaction.TransactionSubCategoryId,
                Description = originalScheduledTransaction.Description,
                AccountType = originalScheduledTransaction.AccountType,
                Amount = originalScheduledTransaction.Amount,
                TransactionKey = originalScheduledTransaction.TransactionKey,
                NextTransactionDate = nextTransactionDate,
                Occurrence = originalScheduledTransaction.Occurrence
            };

            if (saveChanges)
            {
                _dbRepository.QueueAdd<ScheduledTransaction, ScheduledTransactionDto>(newScheduledTransaction);
            }
            else
            {
                await _dbRepository.AddAsync<ScheduledTransaction, ScheduledTransactionDto>(newScheduledTransaction);
            }

            nextTransactionDate = GetNextTransactionDate(nextTransactionDate, originalScheduledTransaction.Occurrence);
        }

        if (saveChanges)
        {
            await _dbRepository.ExecuteQueueAsync();
        }
    }

    public async Task SetScheduledTransactionCompleted(ScheduledTransactionDto updatedScheduledTransaction, bool completed, bool saveChanges)
    {
        var scheduledTransactionInDb =
            (await _scheduledTransactionProvider.GetScheduledTransactions(new ScheduledTransactionQuery { Id = updatedScheduledTransaction.Id })).Single();
        
        scheduledTransactionInDb.Completed = completed;
        
        if (saveChanges)
        {
            await _dbRepository.UpdateAsync<ScheduledTransaction, ScheduledTransactionDto>(scheduledTransactionInDb);
        }
        else
        {
            _dbRepository.QueueUpdate<ScheduledTransaction, ScheduledTransactionDto>(scheduledTransactionInDb);
        }
    }
 
    
    public async Task UpdateScheduledTransaction(ScheduledTransactionDto updatedScheduledTransaction, bool saveChanges)
    {
        _logger.LogInformation(
            "Updating scheduled transaction {Description} ({Id})", 
            updatedScheduledTransaction.Description,
            updatedScheduledTransaction.Id);

        var scheduledTransactionInDb =
            (await _scheduledTransactionProvider.GetScheduledTransactions(
                new ScheduledTransactionQuery
                {
                    Id = updatedScheduledTransaction.Id, 
                    IncludeCompletedTransactions = true
                }))
            .Single();

        scheduledTransactionInDb.TransactionSubCategoryId = updatedScheduledTransaction.TransactionSubCategoryId;
        scheduledTransactionInDb.Description = updatedScheduledTransaction.Description;
        scheduledTransactionInDb.AccountType = updatedScheduledTransaction.AccountType;
        scheduledTransactionInDb.Occurrence = updatedScheduledTransaction.Occurrence;
        scheduledTransactionInDb.Amount = updatedScheduledTransaction.Amount;
        scheduledTransactionInDb.Completed = updatedScheduledTransaction.Completed;

        if (scheduledTransactionInDb.NextTransactionDate != updatedScheduledTransaction.NextTransactionDate)
        {
            scheduledTransactionInDb.NextTransactionDate = updatedScheduledTransaction.NextTransactionDate;

            await DeleteScheduledTransactions(scheduledTransactionInDb.TransactionKey, scheduledTransactionInDb.Id, saveChanges);
            await FillScheduledTransactionForPeriod(scheduledTransactionInDb, DateTime.Now.AddYears(3), saveChanges);
        }

        _dbRepository.QueueUpdate<ScheduledTransaction, ScheduledTransactionDto>(scheduledTransactionInDb);

        if (saveChanges)
        {
            await _dbRepository.ExecuteQueueAsync();
        }
    }

    private async Task DeleteScheduledTransactions(Guid transactionKey, long originalScheduledTransactionId, bool saveChanges)
    {
        var scheduledTransactions = await _scheduledTransactionProvider.GetScheduledTransactions(new ScheduledTransactionQuery
        {
            TransactionKey = transactionKey
        });

        scheduledTransactions = scheduledTransactions.Where(x => x.Id != originalScheduledTransactionId).ToList();

        foreach (var scheduledTransaction in scheduledTransactions)
        {
            _dbRepository.QueueRemove<ScheduledTransaction, ScheduledTransactionDto>(scheduledTransaction);
        }

        if (saveChanges)
        {
            await _dbRepository.ExecuteQueueAsync();
        }
    }

    private static DateTime GetNextTransactionDate(DateTime originalTransactionDate, Occurrence occurrence)
    {
        return occurrence switch
        {
            Occurrence.Daily => originalTransactionDate.Date.AddDays(1),
            Occurrence.Weekly => originalTransactionDate.Date.AddDays(7),
            Occurrence.BiWeekly => originalTransactionDate.Date.AddDays(14),
            Occurrence.Monthly => originalTransactionDate.Date.AddMonths(1),
            Occurrence.BiMonthly => originalTransactionDate.Date.AddMonths(2),
            Occurrence.Annually => originalTransactionDate.Date.AddYears(1),
            Occurrence.BiAnnually => originalTransactionDate.Date.AddYears(2),
            Occurrence.Quarterly => originalTransactionDate.Date.AddMonths(3),
            Occurrence.Semiannually => originalTransactionDate.Date.AddMonths(6),
            _ => throw new ArgumentOutOfRangeException(nameof(occurrence))
        };
    }

    public async Task DeleteScheduledTransaction(ScheduledTransactionDto scheduledTransaction, bool saveChanges)
    {
        _logger.LogInformation(
            "Deleting scheduled transaction {Description} (Id: {Id})", 
            scheduledTransaction.Description,
            scheduledTransaction.Id);
        
        _dbRepository.QueueRemove<ScheduledTransaction, ScheduledTransactionDto>(scheduledTransaction);
        
        if (saveChanges)
        {
            await _dbRepository.ExecuteQueueAsync();
        }
    }

    public async Task SaveChanges()
    {
        await _dbRepository.ExecuteQueueAsync();
    }
}