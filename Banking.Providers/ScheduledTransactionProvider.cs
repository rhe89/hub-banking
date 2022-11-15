using System.Collections.Generic;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.Repository;
using Hub.Shared.Storage.Repository.Core;

namespace Banking.Providers;

public interface IScheduledTransactionProvider
{
    Task<IList<ScheduledTransactionDto>> GetScheduledTransactions();
    Task<IList<ScheduledTransactionDto>> GetScheduledTransactions(ScheduledTransactionQuery scheduledTransactionQuery);
}

public class ScheduledTransactionProvider : IScheduledTransactionProvider
{
    private readonly IHubDbRepository _dbRepository;

    public ScheduledTransactionProvider(IHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }

    public async Task<IList<ScheduledTransactionDto>> GetScheduledTransactions()
    {
        return await GetScheduledTransactions(new ScheduledTransactionQuery());
    }

    public async Task<IList<ScheduledTransactionDto>> GetScheduledTransactions(ScheduledTransactionQuery scheduledTransactionQuery)
    {
        return await _dbRepository.GetAsync<ScheduledTransaction, ScheduledTransactionDto>(GetQueryable(scheduledTransactionQuery));
    }
    
    private static Queryable<ScheduledTransaction> GetQueryable(ScheduledTransactionQuery scheduledTransactionQuery)
    {
        return new Queryable<ScheduledTransaction>
        {
            Query = scheduledTransactionQuery,
            Where = scheduledTransaction =>
                (scheduledTransactionQuery.Id == null || scheduledTransaction.Id == scheduledTransactionQuery.Id) &&
                (scheduledTransactionQuery.AmountRange == null || scheduledTransaction.Amount >= scheduledTransactionQuery.AmountRange[0] && scheduledTransaction.Amount <= scheduledTransactionQuery.AmountRange[1]) &&
                (scheduledTransactionQuery.TransactionKey == null || scheduledTransaction.TransactionKey == scheduledTransactionQuery.TransactionKey) &&
                (scheduledTransactionQuery.TransactionSubCategoryId == null || scheduledTransaction.TransactionSubCategoryId == scheduledTransactionQuery.TransactionSubCategoryId) &&
                (scheduledTransactionQuery.NextTransactionFromDate == null || scheduledTransaction.NextTransactionDate >= scheduledTransactionQuery.NextTransactionFromDate) &&
                (scheduledTransactionQuery.NextTransactionToDate == null || scheduledTransaction.NextTransactionDate <= scheduledTransactionQuery.NextTransactionToDate) &&
                (string.IsNullOrEmpty(scheduledTransactionQuery.Description) || scheduledTransaction.Text.Contains(scheduledTransactionQuery.Description)) &&
                (string.IsNullOrEmpty(scheduledTransactionQuery.AccountType) || scheduledTransaction.AccountType == scheduledTransactionQuery.AccountType) &&
                (scheduledTransactionQuery.IncludeCompletedTransactions || !scheduledTransaction.Completed),
            OrderBy = scheduledTransaction => scheduledTransaction.NextTransactionDate
        };
    }
}