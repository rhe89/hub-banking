using AutoMapper;
using Hub.Shared.Settings;

namespace Banking.Data.AutoMapper;

public static class MapperConfigurationExpressionExtensions
{
    public static void AddEntityMappingProfiles(this IMapperConfigurationExpression mapperConfigurationExpression)
    {
        mapperConfigurationExpression.AddProfile<AccountMapperProfile>();
        mapperConfigurationExpression.AddProfile<AccumulatedAccountBalanceMapperProfile>();
        mapperConfigurationExpression.AddProfile<TransactionMapperProfile>();
        mapperConfigurationExpression.AddProfile<AccountBalanceMapperProfile>();
        mapperConfigurationExpression.AddProfile<ScheduledTransactionMapperProfile>();
        mapperConfigurationExpression.AddProfile<CsvImportMapperProfile>();
        mapperConfigurationExpression.AddProfile<BankMapperProfile>();
        mapperConfigurationExpression.AddProfile<SettingMapperProfile>();
        mapperConfigurationExpression.AddProfile<TransactionCategoryMapperProfile>();
        mapperConfigurationExpression.AddProfile<MonthlyBudgetMapperProfile>();
    }
}