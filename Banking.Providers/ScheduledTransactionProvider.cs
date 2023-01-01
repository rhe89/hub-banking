using System.Collections.Generic;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.Repository.Core;

namespace Banking.Providers;

public interface IScheduledTransactionProvider
{
    Task<IList<ScheduledTransactionDto>> Get();
    Task<IList<ScheduledTransactionDto>> Get(ScheduledTransactionQuery entity);
}

public class ScheduledTransactionProvider : IScheduledTransactionProvider
{
    private readonly IHubDbRepository _dbRepository;

    public ScheduledTransactionProvider(IHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }

    public async Task<IList<ScheduledTransactionDto>> Get()
    {
        return await Get(new ScheduledTransactionQuery());
    }

    public async Task<IList<ScheduledTransactionDto>> Get(ScheduledTransactionQuery entity)
    {
        return await _dbRepository.GetAsync<ScheduledTransaction, ScheduledTransactionDto>(GetQueryable(entity));
    }
    
    private static Queryable<ScheduledTransaction> GetQueryable(ScheduledTransactionQuery query)
    {
        if (query.Id != null)
        {
            query.IncludeCompletedTransactions = true;
        }
        
        return new Queryable<ScheduledTransaction>
        {
            Where = entity =>
                (query.Id == null || query.Id == entity.Id) &&
                (query.AccountId == null || query.AccountId == 0 || query.AccountId == entity.AccountId) &&
                (query.AmountRange == null || entity.Amount >= query.AmountRange[0] && entity.Amount <= query.AmountRange[1]) &&
                (query.TransactionKey == null || entity.TransactionKey == query.TransactionKey) &&
                (query.TransactionSubCategoryId == null || entity.TransactionSubCategoryId == query.TransactionSubCategoryId) &&
                (query.NextTransactionFromDate == null || entity.NextTransactionDate.Date >= query.NextTransactionFromDate.Value.Date) &&
                (query.NextTransactionToDate == null || entity.NextTransactionDate.Date <= query.NextTransactionToDate.Value.Date) &&
                (string.IsNullOrEmpty(query.Description) || entity.Text.Contains(query.Description)) &&
                (string.IsNullOrEmpty(query.AccountType) || entity.Account.AccountType == query.AccountType) &&
                (query.IncludeCompletedTransactions || !entity.Completed),
            Include = entity => entity.Account,
            OrderBy = entity => entity.NextTransactionDate,
            Take = query.Take,
            Skip = query.Skip
        };
    }
}