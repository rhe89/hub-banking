using AutoMapper;

namespace Sbanken.Data.AutoMapper
{
    public static class MapperConfigurationExpressionExtensions
    {
        public static void AddSbankenProfiles(this IMapperConfigurationExpression mapperConfigurationExpression)
        {
            mapperConfigurationExpression.AddProfile<AccountMapperProfile>();
            mapperConfigurationExpression.AddProfile<TransactionMapperProfile>();
            mapperConfigurationExpression.AddProfile<AccountBalanceMapperProfile>();
        }
    }
}