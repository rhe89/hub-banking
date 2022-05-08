using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.SearchParameters;
using Hub.Shared.Storage.Repository.Core;

namespace Banking.Providers;

public interface IRecurringTransactionProvider
{
    Task<IList<RecurringTransactionDto>> GetRecurringTransactions(RecurringTransactionSearchParameters recurringTransactionSearchParameters);
}

public class RecurringTransactionProvider : IRecurringTransactionProvider
{
    private readonly IHubDbRepository _dbRepository;

    public RecurringTransactionProvider(IHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }
    
    public async Task<IList<RecurringTransactionDto>> GetRecurringTransactions(RecurringTransactionSearchParameters recurringTransactionSearchParameters)
    {
        Expression<Func<RecurringTransaction, bool>> predicate = recurringTransaction =>
            (recurringTransactionSearchParameters.RecurringTransactionId == null ||
             recurringTransactionSearchParameters.RecurringTransactionId == recurringTransaction.Id) &&
            (recurringTransactionSearchParameters.AccountIds == null ||
             recurringTransactionSearchParameters.AccountIds.Any(accountId =>
                 accountId == recurringTransaction.AccountId)) &&
            (recurringTransactionSearchParameters.Description == null ||
             recurringTransactionSearchParameters.Description == recurringTransaction.Text) &&
            (string.IsNullOrEmpty(recurringTransactionSearchParameters.Description) ||
             recurringTransactionSearchParameters.Description
                 .Contains(recurringTransactionSearchParameters.Description));
            
        var recurringTransactions = await _dbRepository.WhereAsync<RecurringTransaction, RecurringTransactionDto>(predicate);
        
        if (recurringTransactionSearchParameters.Take != null)
        {
            recurringTransactions = recurringTransactions
                .OrderByDescending(x => x.UpdatedDate)
                .Take(recurringTransactionSearchParameters.Take.Value)
                .ToList();
        }
        
        return recurringTransactions
            .OrderByDescending(x => x.NextTransactionDate)
            .ToList();
    }
}