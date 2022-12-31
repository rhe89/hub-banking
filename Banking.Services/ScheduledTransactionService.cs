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
    Task<ScheduledTransactionDto> Add(ScheduledTransactionDto newScheduledTransaction, bool saveChanges);
    Task SetCompleted(ScheduledTransactionDto updatedScheduledTransaction, bool completed, bool saveChanges);
    Task Update(ScheduledTransactionDto updatedScheduledTransaction, bool saveChanges);
    Task AddOrUpdate(ScheduledTransactionDto scheduledTransaction, bool saveChanges);
    Task Delete(ScheduledTransactionDto scheduledTransaction, bool saveChanges);
    Task FillScheduledTransactionForPeriod(ScheduledTransactionDto originalScheduledTransaction, DateTime periodEnd, bool saveChanges);
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

    public async Task<ScheduledTransactionDto> Add(ScheduledTransactionDto newScheduledTransaction, bool saveChanges)
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

    public async Task FillScheduledTransactionForPeriod(ScheduledTransactionDto originalScheduledTransaction, DateTime periodEnd, bool saveChanges)
    {
        if (originalScheduledTransaction.Occurrence == Occurrence.Once)
        {
            return;
        }

        var nextTransactionDate = GetNextTransactionDate(originalScheduledTransaction.NextTransactionDate, originalScheduledTransaction.Occurrence);

        while (nextTransactionDate <= periodEnd)
        {
            var newScheduledTransaction = new ScheduledTransactionDto
            {
                AccountId = originalScheduledTransaction.AccountId,
                TransactionSubCategoryId = originalScheduledTransaction.TransactionSubCategoryId,
                Description = originalScheduledTransaction.Description,
                Amount = originalScheduledTransaction.Amount,
                TransactionKey = originalScheduledTransaction.TransactionKey,
                NextTransactionDate = nextTransactionDate,
                Occurrence = originalScheduledTransaction.Occurrence
            };

            if (saveChanges)
            {
                await _dbRepository.AddAsync<ScheduledTransaction, ScheduledTransactionDto>(newScheduledTransaction);
            }
            else
            {
                _dbRepository.QueueAdd<ScheduledTransaction, ScheduledTransactionDto>(newScheduledTransaction);
            }

            nextTransactionDate = GetNextTransactionDate(nextTransactionDate, originalScheduledTransaction.Occurrence);
        }

        if (saveChanges)
        {
            await _dbRepository.ExecuteQueueAsync();
        }
    }

    public async Task SetCompleted(ScheduledTransactionDto updatedScheduledTransaction, bool completed, bool saveChanges)
    {
        var scheduledTransactionInDb =
            (await _scheduledTransactionProvider.Get(new ScheduledTransactionQuery { Id = updatedScheduledTransaction.Id })).Single();
        
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
 
    
    public async Task Update(ScheduledTransactionDto updatedScheduledTransaction, bool saveChanges)
    {
        _logger.LogInformation(
            "Updating scheduled transaction {Description} ({Id})", 
            updatedScheduledTransaction.Description,
            updatedScheduledTransaction.Id);

        var scheduledTransactionInDb =
            (await _scheduledTransactionProvider.Get(
                new ScheduledTransactionQuery
                {
                    Id = updatedScheduledTransaction.Id
                }))
            .Single();

        scheduledTransactionInDb.AccountId = updatedScheduledTransaction.AccountId;
        scheduledTransactionInDb.TransactionSubCategoryId = updatedScheduledTransaction.TransactionSubCategoryId;
        scheduledTransactionInDb.Description = updatedScheduledTransaction.Description;
        scheduledTransactionInDb.Occurrence = updatedScheduledTransaction.Occurrence;
        scheduledTransactionInDb.Amount = updatedScheduledTransaction.Amount;
        scheduledTransactionInDb.Completed = updatedScheduledTransaction.Completed;
        scheduledTransactionInDb.NextTransactionDate = updatedScheduledTransaction.NextTransactionDate;

        await DeleteNewerScheduledTransactions(scheduledTransactionInDb.TransactionKey, scheduledTransactionInDb.Id, scheduledTransactionInDb.NextTransactionDate, saveChanges);
        await FillScheduledTransactionForPeriod(scheduledTransactionInDb, DateTime.Now.AddYears(3), saveChanges);

        _dbRepository.QueueUpdate<ScheduledTransaction, ScheduledTransactionDto>(scheduledTransactionInDb);

        if (saveChanges)
        {
            await _dbRepository.ExecuteQueueAsync();
        }
    }

    public async Task AddOrUpdate(ScheduledTransactionDto scheduledTransaction, bool saveChanges)
    {
        var existingScheduledTransaction = (await _scheduledTransactionProvider.Get(new ScheduledTransactionQuery
        {
            Description = scheduledTransaction.Description,
            IncludeCompletedTransactions = true,
        })).FirstOrDefault();

        if (existingScheduledTransaction == null)
        {
            await Add(scheduledTransaction, saveChanges);
        }
        else
        {
            existingScheduledTransaction.AccountId = scheduledTransaction.AccountId;
            existingScheduledTransaction.Amount = scheduledTransaction.Amount;
            existingScheduledTransaction.NextTransactionDate = scheduledTransaction.NextTransactionDate;

            await Update(existingScheduledTransaction, saveChanges);
        }
    }

    private async Task DeleteNewerScheduledTransactions(
        Guid transactionKey, 
        long originalScheduledTransactionId, 
        DateTime notBeforeDate,
        bool saveChanges)
    {
        var scheduledTransactions = await _scheduledTransactionProvider.Get(new ScheduledTransactionQuery
        {
            TransactionKey = transactionKey,
        });

        scheduledTransactions = scheduledTransactions
            .Where(x => x.Id != originalScheduledTransactionId && notBeforeDate < x.NextTransactionDate).ToList();

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

    public async Task Delete(ScheduledTransactionDto scheduledTransaction, bool saveChanges)
    {
        _logger.LogInformation(
            "Deleting scheduled transaction {Description} (Id: {Id})", 
            scheduledTransaction.Description,
            scheduledTransaction.Id);

        await DeleteNewerScheduledTransactions(scheduledTransaction.TransactionKey, scheduledTransaction.Id, scheduledTransaction.NextTransactionDate, saveChanges);

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