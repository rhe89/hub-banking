using System.Linq;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.Repository;

namespace Banking.Services;

public interface IMonthlyBudgetService
{
    Task<MonthlyBudgetDto> AddOrUpdate(MonthlyBudgetDto monthlyBudget, bool saveChanges);
    Task<MonthlyBudgetDto> Add(MonthlyBudgetDto monthlyBudget, bool saveChanges);
    Task<MonthlyBudgetDto> Update(MonthlyBudgetDto existing, MonthlyBudgetDto updated, bool saveChanges);
    Task SaveChanges();
}

public class MonthlyBudgetService : IMonthlyBudgetService
{
    private readonly IMonthlyBudgetProvider _monthlyBudgetProvider;
    private readonly ICacheableHubDbRepository _dbRepository;

    public MonthlyBudgetService(
        IMonthlyBudgetProvider monthlyBudgetProvider,
        ICacheableHubDbRepository dbRepository)
    {
        _monthlyBudgetProvider = monthlyBudgetProvider;
        _dbRepository = dbRepository;
    }
    
    public async Task<MonthlyBudgetDto> AddOrUpdate(MonthlyBudgetDto monthlyBudget, bool saveChanges)
    {
        var existingEntity = (await _monthlyBudgetProvider.Get(new MonthlyBudgetQuery
        {
            Month = monthlyBudget.Month
        })).FirstOrDefault();
        
        if (existingEntity == null)
        {
            return await Add(monthlyBudget, saveChanges);
        }
        else
        {
            return await Update(existingEntity, monthlyBudget, saveChanges);
        }
    }

    public async Task<MonthlyBudgetDto> Add(MonthlyBudgetDto monthlyBudget, bool saveChanges)
    {
        monthlyBudget.Result = CalculateResult(monthlyBudget);

        if (saveChanges)
        {
            await _dbRepository.AddAsync<MonthlyBudget, MonthlyBudgetDto>(monthlyBudget);
        }
        else
        {
            _dbRepository.QueueAdd<MonthlyBudget, MonthlyBudgetDto>(monthlyBudget);
        }

        return monthlyBudget;
    }

    public async Task<MonthlyBudgetDto> Update(MonthlyBudgetDto existing, MonthlyBudgetDto updated, bool saveChanges)
    {
        existing.Income = updated.Income;
        existing.Savings = updated.Savings;
        existing.Mortgage = updated.Mortgage;
        existing.SharedAccountTransactions = updated.SharedAccountTransactions;
        existing.Investments = updated.Investments;
        existing.Bills = updated.Bills;
        
        existing.Result = CalculateResult(existing);

        if (saveChanges)
        {
            await _dbRepository.UpdateAsync<MonthlyBudget, MonthlyBudgetDto>(existing);
        }
        else
        {
            _dbRepository.QueueUpdate<MonthlyBudget, MonthlyBudgetDto>(existing);
        }

        return existing;
    }

    private static decimal CalculateResult(MonthlyBudgetDto existing)
    {
        return existing.Income -
               existing.Bills -
               existing.Investments -
               existing.Mortgage -
               existing.Savings -
               existing.SharedAccountTransactions;
    }

    public async Task SaveChanges()
    {
        await _dbRepository.ExecuteQueueAsync();
    }
}