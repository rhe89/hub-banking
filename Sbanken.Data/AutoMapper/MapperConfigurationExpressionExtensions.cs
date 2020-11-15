using AutoMapper;

namespace Sbanken.Data.AutoMapper
{
    public static class MapperConfigurationExpressionExtensions
    {
        public static IMapperConfigurationExpression AddSbankenProfiles(this IMapperConfigurationExpression mapperConfigurationExpression)
        {
            mapperConfigurationExpression.AddProfile<AccountMapperProfile>();
            mapperConfigurationExpression.AddProfile<TransactionMapperProfile>();
            mapperConfigurationExpression.AddProfile<AccountBalanceMapperProfile>();

            return mapperConfigurationExpression;
        }
    }
}