using AutoMapper;

namespace Banking.Data.AutoMapper;

public static class MapperConfigurationExpressionExtensions
{
    public static void AddEntityMappingProfiles(this IMapperConfigurationExpression mapperConfigurationExpression)
    {
        mapperConfigurationExpression.AddProfile<AccountMapperProfile>();
        mapperConfigurationExpression.AddProfile<TransactionMapperProfile>();
        mapperConfigurationExpression.AddProfile<AccountBalanceMapperProfile>();
        mapperConfigurationExpression.AddProfile<PreferenceMapperProfile>();
        mapperConfigurationExpression.AddProfile<ScheduledTransactionMapperProfile>();
        mapperConfigurationExpression.AddProfile<CsvImportMapperProfile>();
        mapperConfigurationExpression.AddProfile<BankMapperProfile>();
        mapperConfigurationExpression.AddProfile<BankMapperProfile>();
        mapperConfigurationExpression.AddProfile<TransactionCategoryMapperProfile>();
    }
}