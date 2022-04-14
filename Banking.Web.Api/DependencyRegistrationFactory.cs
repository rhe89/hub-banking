using Hub.Shared.Web.Api;
using Hub.Shared.Web.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Banking.Data;
using Banking.Data.AutoMapper;
using Banking.Integration;
using Banking.Providers;

namespace Banking.Web.Api;

public class DependencyRegistrationFactory : DependencyRegistrationFactory<BankingDbContext>
{
    public DependencyRegistrationFactory() : base("SQL_DB_BANKING", "Banking.Data")
    {
    }

    protected override void AddDomainDependencies(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.TryAddTransient<IAccountProvider, AccountProvider>();
        serviceCollection.TryAddTransient<ITransactionProvider, TransactionProvider>();
        serviceCollection.TryAddTransient<IAccountBalanceProvider, AccountBalanceProvider>();
        serviceCollection.AddHubHttpClient<ISbankenConnector, SbankenConnector>();

        serviceCollection.AddAutoMapper(c =>
        {
            c.AddEntityMappingProfiles();
        });
    }
}