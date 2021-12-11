using Hub.Shared.Web.Api;
using Hub.Shared.Web.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sbanken.Data;
using Sbanken.Data.AutoMapper;
using Sbanken.Integration;
using Sbanken.Providers;

namespace Sbanken.Web.Api;

public class DependencyRegistrationFactory : DependencyRegistrationFactory<SbankenDbContext>
{
    public DependencyRegistrationFactory() : base("SQL_DB_SBANKEN", "Sbanken.Data")
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
            c.AddSbankenProfiles();
        });
    }
}